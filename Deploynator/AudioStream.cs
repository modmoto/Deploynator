using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace Deploynator
{
    public class AudioStream
    {
        private readonly EventBus _eventBus;
        private LanguageSetting _languageSetting;

        public AudioStream(EventBus eventBus, LanguageSetting languageSetting)
        {
            _eventBus = eventBus;
            _languageSetting = languageSetting;
            _eventBus.ReleasesTriggered += (_, args) => PlayReleases(args);
            _eventBus.ServiceStarted += (_, _) => Play("Deployment ready, awaiting deployment sequence");
            _eventBus.ReleaseFailed += (_, _) => Play("Release failed, please stay calm and leave the building in an orderly fashion");
            _eventBus.ReleasesSucceeded += (_, args) => PlaySuccessfulDeployments(args as DeploymentResultsArgs);

            _eventBus.SelectedDeloyment += (_, args) =>
            {
                Play($"{(args as SelectReleaseDefinitionArgs)?.ReleaseDefinition.Name} selected for Deployment");
            };

            _eventBus.DeselectedDeloyment += (_, args) =>
            {
                Play($"{(args as SelectReleaseDefinitionArgs)?.ReleaseDefinition.Name} removed from Deployment, bitch please");
            };
        }

        private async Task PlayReleases(EventArgs args)
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
            var config = SpeechConfig.FromSubscription("990a253fc3cb487e8f02867fcd3d86c2", "francecentral");
            config.SpeechSynthesisVoiceName = _languageSetting.Person;
            var synthesizer = new SpeechSynthesizer(config);
            await synthesizer.SpeakTextAsync(message);
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

            await Play("Status report finished! The cake is a lie!:");
        }
    }

    public class LanguageSetting
    {
        public string Person { get; set; } = "en-US-SaraNeural";
    }
}