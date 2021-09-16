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
            eventBus.ReleaseButtonTriggered += (_, _) => OnReleaseButtonTriggered();
            eventBus.ServiceStarted += (_, _) => OnServiceStarted();
            var config = SpeechConfig.FromSubscription("990a253fc3cb487e8f02867fcd3d86c2", "francecentral");
            // config.SpeechSynthesisVoiceName = "en-US-AriaNeural";
            config.SpeechSynthesisVoiceName = "en-US-SaraNeural";
            _synthesizer = new SpeechSynthesizer(config);
        }

        private async Task OnServiceStarted()
        {
            await Play("Deployment ready, awaiting deployment sequence");
        }

        private async void OnReleaseButtonTriggered()
        {
            await Play("Release triggered");
        }

        public async Task Play(string message)
        {
            await _synthesizer.SpeakTextAsync(message);
        }
    }
}