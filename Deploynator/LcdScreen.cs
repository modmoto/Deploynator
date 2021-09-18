using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Linq;
using System.Threading.Tasks;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using Microsoft.Extensions.Logging;

namespace Deploynator
{
    public class LcdScreen
    {
        private readonly ILogger<LcdScreen> _logger;
        private Lcd2004 _lcd;
        private string _lastTextCommand;

        public LcdScreen(EventBus eventBus, ILogger<LcdScreen> logger)
        {
            _logger = logger;

            eventBus.ServiceStarted += InitlializeLcdScreen;

            eventBus.PreselectedDeloyment += (_, args) =>
            {
                var release = (SelectReleaseDefinitionArgs)args;
                WriteText($"{(release.IsSelected ? "X" : "")}{release.Index} {release.ReleaseDefinition.Name}");
            };

            eventBus.SelectedDeloyment += (_, args) =>
            {
                var release = (SelectReleaseDefinitionArgs)args;
                WriteText($"X{release.Index} {release.ReleaseDefinition.Name}");
            };

            eventBus.DeselectedDeloyment += (_, args) =>
            {
                var release = (SelectReleaseDefinitionArgs) args;
                WriteText($"{release.Index} {release.ReleaseDefinition.Name}");
            };

            eventBus.ReleaseLoaded += (_, args) =>
            {
                var first = ((DeployArgs) args).SelectedDeloyments.FirstOrDefault();
                if (first != null)
                {
                    WriteText("1" + first.Name);
                }
                else
                {
                    WriteText("No deployment available");
                }
            };
        }

        private async void InitlializeLcdScreen(object sender, EventArgs eventArgs)
        {
            do
            {
                try
                {
                    var i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
                    var driver = new Pcf8574(i2c);
                    _lcd = new Lcd2004(registerSelectPin: 0,
                        enablePin: 2,
                        dataPins: new int[] { 4, 5, 6, 7 },
                        backlightPin: 3,
                        backlightBrightness: 0.1f,
                        readWritePin: 1,
                        controller: new GpioController(PinNumberingScheme.Logical, driver));
                    _logger.LogInformation("LCD Screen initlialized");
                    WriteText(_lastTextCommand);
                }
                catch (Exception)
                {
                    _logger.LogDebug("Failed to init LCD Screen, waiting another second");
                    await Task.Delay(1000);
                }
            } while (_lcd != null);
        }

        private void WriteText(string text)
        {
            if (_lcd != null)
            {
                _lcd.Clear();
                _lcd.SetCursorPosition(0, 0);
                var words = text.Split(" ");
                var line = "";
                foreach (var word in words)
                {
                    if (line.Length + word.Length > 16)
                    {
                        _lcd.Write(line);
                        _lcd.SetCursorPosition(0, 1);
                        line = word;
                    }
                    else
                    {
                        line += $"{word} ";
                    }

                }

                _lcd.Write(line);
            }
            else
            {
                _lastTextCommand = text;
                _logger.LogInformation($"LCD: {text}");
            }
        }
    }
}