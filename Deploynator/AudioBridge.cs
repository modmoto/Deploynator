using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace Deploynator
{
    public class AudioBridge
    {
        private readonly EventBus _eventBus;
        private SpeechSynthesizer _synthesizer;
        private LanguageArgs _languageArgs;

        public AudioBridge(EventBus eventBus)
        {
            _eventBus = eventBus;

            CreateSynthi(new LanguageArgs("en-US-SaraNeural"));

            _eventBus.ReleasesTriggered += PlayReleasesTriggered;
            _eventBus.ServiceStarted += PlayServiceStarted;
            _eventBus.ReleaseFailed += PlayReleaseFailed;
            _eventBus.ReleasesSucceeded += PlayReleasesSucceeded;
            _eventBus.LanguageChanged += (_, args) => CreateSynthi(args as LanguageArgs);

            _eventBus.SelectedDeloyment += PlaySelectedSelectedDeloyment;
            _eventBus.DeselectedDeloyment += PlayDeselectedDeloyment;
            _eventBus.WaitingSequenceStarted += PlayWaitingSequenceStarted;
            _eventBus.FoundJoke += PlayFoundJoke;
        }

        private async void PlayFoundJoke(object sender, EventArgs e)
        {
            await Play(((JokeArgs)e).Joke);
            _eventBus.OnJokeFinished();
        }

        private async void PlayWaitingSequenceStarted(object sender, EventArgs e)
        {
            await Play("To ease your pain of waiting, let me tell you some excellent jokes:");
            await Task.Delay(500);
        }

        private async void PlayDeselectedDeloyment(object sender, EventArgs args)
        {
            await Play($"{(args as SelectReleaseDefinitionArgs)?.ReleaseDefinition.Name} removed from Deployment, bitch please");
        }

        private async void PlaySelectedSelectedDeloyment(object sender, EventArgs args)
        {
            await Play($"{(args as SelectReleaseDefinitionArgs)?.ReleaseDefinition.Name} selected for Deployment");
        }

        private async void PlayReleasesSucceeded(object sender, EventArgs args)
        {
            await PlaySuccessfulDeployments(args as DeploymentResultsArgs);
        }

        private async void PlayReleaseFailed(object sender, EventArgs e)
        {
            await Play("Release failed, please stay calm and leave the building in an orderly fashion");
        }

        private async void PlayServiceStarted(object sender, EventArgs e)
        {
            await Play("Deployment ready, awaiting deployment sequence");
        }

        private void CreateSynthi(LanguageArgs languageArgs)
        {
            var config = SpeechConfig.FromSubscription("990a253fc3cb487e8f02867fcd3d86c2", "francecentral");
            config.SpeechSynthesisVoiceName = languageArgs.NewLanguage;
            _languageArgs = languageArgs;
            _synthesizer = new SpeechSynthesizer(config);
        }

        private async void PlayReleasesTriggered(object sender, EventArgs args)
        {
            var deployArgs = args as DeployArgs;
            var deloyments = deployArgs.SelectedDeloyments;
            var strings = string.Join(", ", deloyments.Select(d => d.Name));
            await Play($"Starting to deploy services: {strings} in t minus 5 seconds.");
            var countDown = new[] {"5", "4", "3", "2", "1", "Deploy!"};
            foreach (var countDownValue in countDown)
            {
                await Play(countDownValue);
                await Task.Delay(200);
            }
            _eventBus.OnReleaseCountdownFinished(deloyments);
        }

        public async Task Play(string message)
        {
            await _synthesizer.SpeakTextAsync(message);
        }

        private async Task PlaySuccessfulDeployments(DeploymentResultsArgs deploymentResultsArgs)
        {
            await Play("Status report incoming, Master:");
            
            foreach (var deploymentResultsArg in deploymentResultsArgs.DeploymentResults.OrderBy(x => x.Deployed).ToList())
            {
                await Task.Delay(1000);

                var resultMessage = deploymentResultsArg.Deployed ? "Success!" : "Failed!";
                await Play(
                    $"{deploymentResultsArg.ReleaseName} finished with status: {resultMessage}");
            }

            if (_languageArgs.NewLanguage == "es-MX-JorgeNeural")
            {
                await Play("Status report finished! Happy cinco de mayo!");

            }
            else
            {
                await Play("Status report finished! The cake is a lie!:");
            }
        }
    }
}