using System.Device.Gpio;
using System.Device.I2c;
using System.Net;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;

namespace Deploynator
{
    public class LcdScreen
    {
        private readonly Lcd2004 _lcd;

        public LcdScreen(EventBus eventBus)
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

            eventBus.PreselectedDeloyment += (_, args) =>
            {
                WriteText((args as SelectReleaseDefinitionArgs)?.ReleaseDefinition.Name);
            };


        }

        private void WriteText(string text)
        {
            _lcd.Clear();
            _lcd.SetCursorPosition(0, 0);
            _lcd.Write(text);
        }
    }
}