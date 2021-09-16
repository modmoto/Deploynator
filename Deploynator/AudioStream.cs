using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace Deploynator
{
    public class AudioStream
    {
        private readonly SpeechSynthesizer _synthesizer;

        public AudioStream(EventBus eventBus)
        {
            eventBus.ReleaseTriggered += OnReleaseTriggered;
            var config = SpeechConfig.FromSubscription("990a253fc3cb487e8f02867fcd3d86c2", "francecentral");
            // config.SpeechSynthesisVoiceName = "en-US-AriaNeural";
            config.SpeechSynthesisVoiceName = "en-US-SaraNeural";
            _synthesizer = new SpeechSynthesizer(config);
        }

        private async void OnReleaseTriggered(object o, EventArgs eventArgs)
        {
            await Play("Release triggered");
        }

        public async Task Play(string message)
        {
            await _synthesizer.SpeakTextAsync(message);
        }
    }
}