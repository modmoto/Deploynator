using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deploynator
{
    public class RaspberryButtonsBridge : BackgroundService
    {
        private readonly ILogger<RaspberryButtonsBridge> _logger;
        private readonly EventBus _eventBus;
        private GpioController _controller;
        private bool _releaseButtonDown;
        private bool _upButtonDown;
        private bool _downButtonDown;
        private bool _upAndDownButtonDown;
        private bool _selectButtonDown;
        private bool _deselectButtonDown;
        private bool _selectAndDeseceltButtonDown;

        private const int ReleaseButton = 38;

        private const int UpButton = 33;
        private const int DownButton = 31;
        private const int SelectButton = 35;
        private const int DeselectButton = 37;

        public RaspberryButtonsBridge(ILogger<RaspberryButtonsBridge> logger, EventBus eventBus, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _eventBus = eventBus;

            // Hack to get the DI to trigger
            serviceProvider.GetService(typeof(AudioBridge));
            serviceProvider.GetService(typeof(LcdScreenBridge));
            serviceProvider.GetService(typeof(DeploymentHandler));
        }

        private async Task InitializeRaspberryPins()
        {
            var couldOpenPins = false;
            do
            {
                try
                {
                    _controller = new GpioController(PinNumberingScheme.Board);

                    _controller.OpenPin(ReleaseButton, PinMode.InputPullUp);
                    _controller.OpenPin(UpButton, PinMode.InputPullUp);
                    _controller.OpenPin(DownButton, PinMode.InputPullUp);
                    _controller.OpenPin(SelectButton, PinMode.InputPullUp);
                    _controller.OpenPin(DeselectButton, PinMode.InputPullUp);
                    _logger.LogInformation("RaspberryBridge initlialized");
                    couldOpenPins = true;
                }
                catch (Exception)
                {
                    _logger.LogInformation("Failed to init RaspiBridge, waiting another second");
                    await Task.Delay(1000);
                }
            } while (!couldOpenPins);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Raspberry Bridge started, will init bridge now");
            _eventBus.OnServiceStarted();
            await InitializeRaspberryPins();

            do
            {
                try
                {
                    CheckButtonState(ReleaseButton, ref _releaseButtonDown, () => _eventBus.OnReleaseButtonTriggered());
                    CheckButtonState(DownButton, ref _downButtonDown, () => _eventBus.OnRightButtonTriggered());
                    CheckButtonState(UpButton, ref _upButtonDown, () => _eventBus.OnLeftButtonTriggered());
                    CheckButtonState(SelectButton, ref _selectButtonDown, () => _eventBus.OnSelectButtonTriggered());
                    CheckButtonState(DeselectButton, ref _deselectButtonDown, () => _eventBus.OnDeselectButtonTriggered());

                    CheckDoubleButtonState(
                        _deselectButtonDown,
                        _selectButtonDown,
                        ref _selectAndDeseceltButtonDown,
                        () => _eventBus.OnSelectAndDeselectButtonTriggered(),
                        () => _eventBus.OnSelectAndDeselectButtonReleased());
                    CheckDoubleButtonState(
                        _upButtonDown,
                        _downButtonDown,
                        ref _upAndDownButtonDown,
                        () => _eventBus.OnLeftAndRightButtonTriggered(),
                        () => _eventBus.OnLeftAndRightButtonReleased());

                    await Task.Delay(20, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "dead");
                }

            } while (!cancellationToken.IsCancellationRequested);

            Clean();
        }

        private void CheckDoubleButtonState(
            bool button1,
            bool button2,
            ref bool buttonIsDown,
            Action enableFunction,
            Action disableFunction)
        {
            if (button1 && button2 & !buttonIsDown)
            {
                buttonIsDown = true;
                enableFunction.Invoke();
            }
            else
            {
                if (buttonIsDown && button1 && button2)
                {
                    buttonIsDown = false;
                    disableFunction.Invoke();
                }
            }
        }

        private void CheckButtonState(int button, ref bool buttonVar, Action eventTrigger)
        {
            if (_controller.Read(button) == false && !buttonVar)
            {
                buttonVar = true;
                _logger.LogInformation($"triggered {button}");
                eventTrigger.Invoke();
            }

            if (_controller.Read(button) == true && buttonVar)
            {
                buttonVar = false;
            }
        }

        private void Clean()
        {
            _controller.ClosePin(ReleaseButton);
            _controller.ClosePin(DeselectButton);
            _controller.ClosePin(SelectButton);
            _controller.ClosePin(DownButton);
            _controller.ClosePin(UpButton);
        }
    }
}