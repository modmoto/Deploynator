using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deploynator
{
    public class RaspberryHandler : IHostedService
    {
        private bool _releaseButtonPressed;
        private ILogger<RaspberryHandler> _logger;
        private const int Led1 = 10;
        private const int ReleaseButton = 26;


        public RaspberryHandler(ILogger<RaspberryHandler> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            do
            {
                try
                {
                    var controller = new GpioController(PinNumberingScheme.Board);

                    controller.OpenPin(Led1, PinMode.Output);
                    controller.OpenPin(ReleaseButton, PinMode.InputPullUp);

                    if (controller.Read(ReleaseButton) == false && _releaseButtonPressed == false)
                    {
                        _releaseButtonPressed = true;
                        // _eventBus.OnReleaseTriggered();
                        _logger.LogInformation("Release triggered");
                    }
                    else
                    {
                        if (controller.Read(ReleaseButton) == true)
                        {
                            _releaseButtonPressed = false;
                        }
                    }

                    await Task.Delay(5);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Rasbi dead");
                }

                await Task.Delay(5, cancellationToken);
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        // private void TurnOffLed(int pin)
        // {
        //     _controller.Write(pin, PinValue.Low);
        // }
        //
        // private void TurnOnLed(int pin)
        // {
        //     _controller.Write(pin, PinValue.High);
        // }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // _controller.ClosePin(Led1);
            // _controller.ClosePin(ReleaseButton);
            return Task.CompletedTask;
        }
    }
}