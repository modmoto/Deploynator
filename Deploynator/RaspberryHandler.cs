using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deploynator
{
    public class RaspberryHandler : IHostedService
    {
        private ILogger<RaspberryHandler> _logger;


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

                    // controller.OpenPin(10, PinMode.Output);
                    controller.ClosePin(10);
                    controller.ClosePin(26);
                    controller.OpenPin(26, PinMode.InputPullUp);

                    if (controller.Read(26) == false)
                    {
                        _logger.LogInformation("Release triggered");
                    }
                    else
                    {
                        _logger.LogInformation("no");
                    }

                    await Task.Delay(5, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "dead");
                }
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