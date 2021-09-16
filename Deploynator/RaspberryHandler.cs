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
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            do
            {
                try
                {
                    if (_controller.Read(ReleaseButton) == false && _releaseButtonDown == false)
                    {
                        _eventBus.OnReleaseButtonTriggered();
                        _releaseButtonDown = true;
                        _logger.LogInformation("triggered Release");
                        await Task.Delay(100, cancellationToken);
                    }
                    else
                    {
                        if (_controller.Read(ReleaseButton) == true)
                        {
                            _releaseButtonDown = false;
                            _eventBus.OnReleaseButtonReleased();
                            _logger.LogInformation("released Release");
                        }
                    }

                    _logger.LogInformation("nono");
                    await Task.Delay(500, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "dead");
                }
            } while (!cancellationToken.IsCancellationRequested);

            Clean();
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