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
        private const int Led1 = 10;
        private const int ReleaseButton = 26;

        public RaspberryHandler(ILogger<RaspberryHandler> logger, EventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
            _controller = new GpioController(PinNumberingScheme.Board);

            _controller.OpenPin(Led1, PinMode.Output);
            _controller.OpenPin(ReleaseButton, PinMode.InputPullUp);

            _eventBus.ReleaseFailed += (_, _) => OnReleaseFailed();
            _eventBus.ReleaseSuceeded += (_, _) => OnReleaseSuceeded();
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
                    CheckButtonState();

                    await Task.Delay(20, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "dead");
                }

            } while (!cancellationToken.IsCancellationRequested);

            Clean();
        }

        private void CheckButtonState()
        {
            if (_controller.Read(ReleaseButton) == false && !_releaseButtonDown)
            {
                _releaseButtonDown = true;
                _logger.LogInformation("triggered Release");
                _eventBus.OnReleaseButtonTriggered();
            }

            if (_controller.Read(ReleaseButton) == true && _releaseButtonDown)
            {
                _releaseButtonDown = false;
                _logger.LogInformation("released Release");
                _eventBus.OnReleaseButtonReleased();
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