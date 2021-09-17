using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deploynator
{
    public class RaspberryHandler : BackgroundService
    {
        private readonly ILogger<RaspberryHandler> _logger;
        private readonly EventBus _eventBus;
        private readonly GpioController _controller;
        private bool _releaseButtonDown;
        private bool _upButtonDown;
        private bool _downButtonDown;
        private bool _selectButtonDown;
        private bool _deselectButtonDown;
        private const int Led1 = 10;
        private const int ReleaseButton = 26;
        private const int UpButton = 36;
        private const int DownButton = 38;
        private const int SelectButton = 40;
        private const int DeselectButton = 37;

        public RaspberryHandler(ILogger<RaspberryHandler> logger, EventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
            _controller = new GpioController(PinNumberingScheme.Board);

            _controller.OpenPin(Led1, PinMode.Output);
            _controller.OpenPin(ReleaseButton, PinMode.InputPullUp);
            _controller.OpenPin(UpButton, PinMode.InputPullUp);
            _controller.OpenPin(DownButton, PinMode.InputPullUp);
            _controller.OpenPin(SelectButton, PinMode.InputPullUp);
            _controller.OpenPin(DeselectButton, PinMode.InputPullUp);

            _eventBus.ReleaseFailed += (_, _) => OnReleaseFailed();
            _eventBus.ReleaseSucceeded += (_, _) => OnReleaseSuceeded();
        }

        private void OnReleaseSuceeded()
        {
            // led an
        }

        private void OnReleaseFailed()
        {
            //led an
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service started");
            _eventBus.OnServiceStarted();

            do
            {
                try
                {
                    CheckButtonState(ReleaseButton, ref _releaseButtonDown, () => _eventBus.OnReleaseButtonTriggered());
                    CheckButtonState(DownButton, ref _downButtonDown, () => _eventBus.OnDownButtonTriggered());
                    CheckButtonState(UpButton, ref _upButtonDown, () => _eventBus.OnUpButtonTriggered());
                    CheckButtonState(SelectButton, ref _selectButtonDown, () => _eventBus.OnSelectButtonTriggered());
                    CheckButtonState(DeselectButton, ref _deselectButtonDown, () => _eventBus.OnDeselectButtonTriggered());

                    await Task.Delay(20, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "dead");
                }

            } while (!cancellationToken.IsCancellationRequested);

            Clean();
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
                _logger.LogInformation($"released {button}");
            }
        }

        private void TurnOffLed(int pin)
        {
            _controller.Write(pin, PinValue.Low);
        }

        private void TurnOnLed(int pin)
        {
            _controller.Write(pin, PinValue.High);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Clean();
            return Task.CompletedTask;
        }

        private void Clean()
        {
            _controller.ClosePin(Led1);
            _controller.ClosePin(ReleaseButton);
        }
    }
}