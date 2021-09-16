using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deploynator
{
    public class RaspberryHandler : IHostedService
    {
        private readonly ILogger<RaspberryHandler> _logger;
        private readonly EventBus _eventBus;
        private readonly GpioController _controller;
        private bool _releaseButtonPressed;
        private const int Led1 = 10;
        private const int ReleaseButton = 26;

        public RaspberryHandler(ILogger<RaspberryHandler> logger, EventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
            _controller = new GpioController(PinNumberingScheme.Board);

            _controller.OpenPin(Led1, PinMode.Output);
            _controller.OpenPin(ReleaseButton, PinMode.InputPullUp);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while(true)
            {
                CheckReleaseButtonPressed();

                await Task.Delay(5);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private void CheckReleaseButtonPressed()
        {
            if (_controller.Read(ReleaseButton) == false && _releaseButtonPressed == false)
            {
                _releaseButtonPressed = true;
                _eventBus.OnReleaseTriggered();
                _logger.LogInformation("Release triggered");
            }
            else
            {
                if (_controller.Read(ReleaseButton) == true)
                {
                    _releaseButtonPressed = false;
                }
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
            _controller.ClosePin(Led1);
            _controller.ClosePin(ReleaseButton);
            return Task.CompletedTask;
        }
    }
}