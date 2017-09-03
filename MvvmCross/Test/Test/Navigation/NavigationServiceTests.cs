using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.Platform;
using MvvmCross.Core.ViewModels;
using MvvmCross.Test.Core;
using MvvmCross.Test.Mocks.Dispatchers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace MvvmCross.Test.Navigation
{
    [TestFixture]
    public class NavigationServiceTests
        : MvxIoCSupportingTest
    {
        private const string TestParam = "test";
        private MvxNavigationService _navigationService;
//        private SimpleResultTestViewModel _targetedResultTestViewModel;
        private ITestViewModel _targetedTestViewModel;
        protected Mock<NavigationMockDispatcher> MockDispatcher { get; set; }

        [SetUp]
        public void SetupTest()
        {
            Setup();

            SetInvariantCulture();
        }

        protected override void AdditionalSetup()
        {
            base.AdditionalSetup();

            var mockLocator = new Mock<IMvxViewModelLocator>();
            mockLocator.Setup(
                m => m.Load(It.Is<Type>(t => t == typeof(SimpleTestViewModel)), It.IsAny<IMvxBundle>(), It.IsAny<IMvxBundle>())).Returns(() => new SimpleTestViewModel());
            mockLocator.Setup(
                m => m.Load(It.Is<Type>(t => t == typeof(SimpleResultTestViewModel)), It.IsAny<IMvxBundle>(), It.IsAny<IMvxBundle>())).Returns(() => new SimpleResultTestViewModel());
            mockLocator.Setup(
                m => m.Reload(It.IsAny<IMvxViewModel>(), It.IsAny<IMvxBundle>(), It.IsAny<IMvxBundle>())).Returns(() => new SimpleTestViewModel());
            mockLocator.Setup(
                m => m.Reload(It.IsAny<IMvxViewModel<TestResult>>(), It.IsAny<IMvxBundle>(), It.IsAny<IMvxBundle>())).Returns(() => new SimpleResultTestViewModel());

            var mockCollection = new Mock<IMvxViewModelLocatorCollection>();
            mockCollection.Setup(m => m.FindViewModelLocator(It.IsAny<MvxViewModelRequest>()))
                          .Returns(() => mockLocator.Object);

            Ioc.RegisterSingleton(mockLocator.Object);

            var loader = new MvxViewModelLoader(mockCollection.Object);
            MockDispatcher = new Mock<NavigationMockDispatcher>(MockBehavior.Loose) { CallBase = true };
            _navigationService = new MvxNavigationService(null, loader)
            {
                ViewDispatcher = MockDispatcher.Object,
            };
            //Ioc.RegisterSingleton<IMvxNavigationService>(navigationService);
            Ioc.RegisterSingleton<IMvxStringToTypeParser>(new MvxStringToTypeParser());
        }

        [Test]
        public async Task Test_NavigateNoBundle()
        {
            await _navigationService.Navigate<SimpleTestViewModel>();

            MockDispatcher.Verify(
                x => x.ShowViewModel(It.Is<MvxViewModelRequest>(t => t.ViewModelType == typeof(SimpleTestViewModel))),
                Times.Once);
        }

        [Test]
        public async Task Test_NavigateWithBundle()
        {
            var mockVm = new Mock<SimpleTestViewModel>();

            var bundle = new MvxBundle();
            bundle.Write(new { hello = "world" });

            await _navigationService.Navigate(mockVm.Object, bundle);

            mockVm.Verify(vm => vm.Initialize(), Times.Once);
            //mockVm.Verify(vm => vm.Init(), Times.Once);

            //TODO: fix NavigationService not allowing parameter values in request and only presentation values
            //mockVm.Verify(vm => vm.Init(It.Is<string>(s => s == "world")), Times.Once);
        }

        [Test]
        public async Task Test_NavigateViewModelInstance()
        {
            var mockVm = new Mock<SimpleTestViewModel>();

            await _navigationService.Navigate(mockVm.Object);

            mockVm.Verify(vm => vm.Initialize(), Times.Once);
            //mockVm.Verify(vm => vm.Init(), Times.Once);
            Assert.IsTrue(MockDispatcher.Object.Requests.Count > 0);
        }

        [Test]
        public async Task Test_FiresBeforeNavigate()
        {
            await Test_Navigate(async () =>
            {
                await _navigationService.Navigate<SimpleTestViewModel>();
            });
        }

        [Test]
        public async Task Test_FiresBeforeNavigateParam()
        {
            await Test_Navigate(async () =>
            {
                await _navigationService.Navigate<SimpleTestViewModel, string>(TestParam);
            });
        }

        [Test]
        public async Task Test_FiresBeforeNavigateWithResult()
        {
            await Test_Navigate(async () =>
            {
                return await _navigationService.Navigate<SimpleResultTestViewModel, TestResult>(cancellationToken: new CancellationTokenSource(500).Token);
            });
        }

        [Test]
        public async Task Test_FiresBeforeNavigateParamWithResult()
        {
            await Test_Navigate(async () =>
            {
                return await _navigationService.Navigate<SimpleResultTestViewModel, string, TestResult>(TestParam);
            });
        }

        [Test]
        public async Task Test_PrepareShow()
        {
            bool preparedBefore = false;
            MockDispatcher.Setup(
                x => x.ShowViewModel(It.IsAny<MvxViewModelRequest>())).Callback<MvxViewModelRequest>(mnd =>
            {
                Assert.IsTrue(_targetedTestViewModel.IsPrepared);
                preparedBefore = true;
            });

            await Test_Navigate(() => _navigationService.Navigate<SimpleTestViewModel>());

            Assert.IsTrue(preparedBefore);
        }

        [Test]
        public async Task Test_PrepareShowParam()
        {
            bool preparedBefore = false;
            MockDispatcher.Setup(
                x => x.ShowViewModel(It.IsAny<MvxViewModelRequest>())).Callback<MvxViewModelRequest>(mnd =>
            {
                Assert.That(_targetedTestViewModel.PrepareParam, Is.EqualTo(TestParam));
                preparedBefore = true;
            });

            await Test_Navigate(() => _navigationService.Navigate<SimpleTestViewModel, string>(TestParam));

            Assert.IsTrue(preparedBefore);
        }

        [Test]
        public async Task Test_PrepareShowWithResult()
        {
            bool preparedBefore = false;
            MockDispatcher.Setup(
                x => x.ShowViewModel(It.IsAny<MvxViewModelRequest>())).Callback<MvxViewModelRequest>(mnd =>
            {
                Assert.IsTrue(_targetedTestViewModel.IsPrepared);
                preparedBefore = true;
            });

            await Test_Navigate(async () =>
            {
                return await _navigationService.Navigate<SimpleResultTestViewModel, TestResult>(cancellationToken: new CancellationTokenSource(500).Token);
            });

            Assert.IsTrue(preparedBefore);
        }

        [Test]
        public async Task Test_PrepareShowParamWithResult()
        {
            bool preparedBefore = false;
            MockDispatcher.Setup(
                x => x.ShowViewModel(It.IsAny<MvxViewModelRequest>())).Callback<MvxViewModelRequest>(mnd =>
            {
                Assert.That(_targetedTestViewModel.PrepareParam, Is.EqualTo(TestParam));
                preparedBefore = true;
            });

            await Test_Navigate(async () =>
            {
                return await _navigationService.Navigate<SimpleResultTestViewModel, string, TestResult>(TestParam);
            });

            Assert.IsTrue(preparedBefore);
        }

        [Test]
        public async Task Test_ShowInitialize()
        {
            MockDispatcher.Setup(
                x => x.ShowViewModel(It.IsAny<MvxViewModelRequest>())).Callback<MvxViewModelRequest>(mnd =>
            {
                Assert.IsFalse(_targetedTestViewModel.IsInitialized);
            });

            await Test_Navigate(() => _navigationService.Navigate<SimpleTestViewModel>());

            Assert.IsTrue(_targetedTestViewModel.IsInitialized);
        }

        [Test]
        public async Task Test_ShowInitializeParam()
        {
            MockDispatcher.Setup(
                x => x.ShowViewModel(It.IsAny<MvxViewModelRequest>())).Callback<MvxViewModelRequest>(mnd =>
            {
                Assert.IsFalse(_targetedTestViewModel.IsInitialized);
            });

            await Test_Navigate(() => _navigationService.Navigate<SimpleTestViewModel, string>(TestParam));

            Assert.IsTrue(_targetedTestViewModel.IsInitialized);
        }

        [Test]
        public async Task Test_ShowInitializeWithResult()
        {
            bool preparedBefore = false;
            MockDispatcher.Setup(
                x => x.ShowViewModel(It.IsAny<MvxViewModelRequest>())).Callback<MvxViewModelRequest>(mnd =>
            {
                Assert.IsFalse(_targetedTestViewModel.IsInitialized);
            });

            await Test_Navigate(async () =>
            {
                return await _navigationService.Navigate<SimpleResultTestViewModel, TestResult>(cancellationToken: new CancellationTokenSource(500).Token);
            });

            Assert.IsTrue(_targetedTestViewModel.IsInitialized);
        }

        [Test]
        public async Task Test_ShowInitializeParamWithResult()
        {
            bool preparedBefore = false;
            MockDispatcher.Setup(
                x => x.ShowViewModel(It.IsAny<MvxViewModelRequest>())).Callback<MvxViewModelRequest>(mnd =>
            {
                Assert.IsFalse(_targetedTestViewModel.IsInitialized);
            });

            await Test_Navigate(async () =>
            {
                return await _navigationService.Navigate<SimpleResultTestViewModel, string, TestResult>(TestParam);
            });

            Assert.IsTrue(_targetedTestViewModel.IsInitialized);
        }

        [Test]
        public async Task Test_FiresAfterNavigate()
        {
            await Test_FiresAfterNavigate(async () =>
            {
                await _navigationService.Navigate<SimpleTestViewModel>();
            });
        }

        [Test]
        public async Task Test_FiresAfterNavigateParam()
        {
            await Test_FiresAfterNavigate(async () =>
            {
                await _navigationService.Navigate<SimpleTestViewModel, string>(TestParam);
            });
        }

        [Test]
        public async Task Test_FiresAfterNavigateWithResult()
        {
            await Test_FiresAfterNavigate(async () =>
            {
                return await _navigationService.Navigate<SimpleResultTestViewModel, TestResult>(cancellationToken: new CancellationTokenSource(500).Token);
            });
        }

        [Test]
        public async Task Test_FiresAfterNavigateParamWithResult()
        {
            await Test_FiresAfterNavigate(async () =>
            {
                return await _navigationService.Navigate<SimpleResultTestViewModel, string, TestResult>(TestParam);
            });
        }

        private async Task Test_FiresAfterNavigate(Func<Task> navigationAct)
        {
            await Test_FiresAfterNavigate(async () =>
            {
                await navigationAct();
                return new TestResult(0, string.Empty);
            });
        }

        private async Task<TestResult> Test_FiresAfterNavigate(Func<Task<TestResult>> navigationAct)
        {
            _navigationService.AfterNavigate += (sender, args) =>
            {
                var testViewModel = args.ViewModel as ITestViewModel;
                Assert.IsTrue(testViewModel.IsInitialized);
            };

            return await Test_Navigate(async () => await navigationAct());
        }

        private async Task Test_Navigate(Func<Task> navigationAct)
        {
            _targetedTestViewModel = null;
            _navigationService.BeforeNavigate +=
                (sender, args) => _targetedTestViewModel = args.ViewModel as SimpleTestViewModel;

            await navigationAct();

            Assert.That(_targetedTestViewModel, Is.Not.Null);
        }

        private async Task<TResult> Test_Navigate<TResult>(Func<Task<TResult>> navigationAct)
        {
            _targetedTestViewModel = null;
            _navigationService.BeforeNavigate +=
                (sender, args) =>
                {
                    _targetedTestViewModel = args.ViewModel as ITestViewModel;
                };
            _navigationService.AfterNavigate += (sender, args) =>
            {
                if (_targetedTestViewModel is SimpleResultTestViewModel resultViewModel)
                {
                    resultViewModel.Done(new TestResult(0, "test"));
                }
                
            };

            var result = await navigationAct();

            Assert.That(_targetedTestViewModel, Is.Not.Null);

            return result;
        }

        public class SimpleTestViewModel : MvxViewModel, IMvxViewModel<string>, ITestViewModel
        {
            public string PrepareParam { get; private set; }
            public bool IsPrepared { get; private set; }
            public string InitParam { get; private set; }
            public bool IsInitialized { get; private set; }

            public virtual void Init()
            {
            }

            public virtual void Init(string hello)
            {
                InitParam = hello;
            }

            public override Task Initialize()
            {
                IsInitialized = true;
                return base.Initialize();
            }

            public void Prepare(string parameter)
            {
                PrepareParam = parameter;
                IsPrepared = true;
            }

            public void Prepare()
            {
                IsPrepared = true;
            }
        }

        public class SimpleResultTestViewModel : MvxViewModelResult<TestResult>, IMvxViewModel<string, TestResult>, ITestViewModel
        {
            public string PrepareParam { get; private set; }
            public bool IsPrepared { get; private set; }
            public bool IsInitialized { get; private set; }

            public override Task Initialize()
            {
                IsInitialized = true;
                return base.Initialize();
            }

            public void Done(TestResult result)
            {
                CloseCompletionSource.TrySetResult(result);
            }

            public void Prepare(string parameter)
            {
                PrepareParam = parameter;
                IsPrepared = true;
            }

            public void Prepare()
            {
                IsPrepared = true;
            }
        }

        interface ITestViewModel
        {
            string PrepareParam { get; }
            bool IsPrepared { get; }
            bool IsInitialized { get; }
        }

        public class TestResult
        {
            public TestResult(int key, string value)
            {
                Key = key;
                Value = value;
            }

            public int Key { get; set; }
            public string Value { get; set; }
        }
    }
}
