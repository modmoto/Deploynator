using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
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
        private readonly Lcd2004 _lcd;
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

            var i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
            var driver = new Pcf8574(i2c);
            _lcd = new Lcd2004(registerSelectPin: 0,
                enablePin: 2,
                dataPins: new int[] { 4, 5, 6, 7 },
                backlightPin: 3,
                backlightBrightness: 0.1f,
                readWritePin: 1,
                controller: new GpioController(PinNumberingScheme.Logical, driver));
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
                    WriteText();

                    await Task.Delay(20, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "dead");
                }

            } while (!cancellationToken.IsCancellationRequested);

            Clean();
        }

        private void WriteText()
        {
            _lcd.Clear();
            _lcd.SetCursorPosition(0, 0);
            _lcd.Write(DateTime.Now.ToShortTimeString());
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