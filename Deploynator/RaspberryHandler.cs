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
        private readonly ILogger<RaspberryHandler> _logger;
        private readonly GpioController _controller;
        private const int Led1 = 10;
        private const int Button1 = 26;

        public RaspberryHandler(ILogger<RaspberryHandler> logger)
        {
            _logger = logger;
            _controller = new GpioController(PinNumberingScheme.Board);

            _controller.OpenPin(Led1, PinMode.Output);
            _controller.OpenPin(Button1, PinMode.InputPullUp);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while(true)
            {
                if (_controller.Read(Button1) == false)
                {
                    _logger.LogInformation("leLldldldld");
                    await Task.Delay(100);
                }
                else
                {
                    _logger.LogInformation("lu");
                }

                await Task.Delay(10);
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
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
            _controller.ClosePin(Button1);
            return Task.CompletedTask;
        }
    }
}