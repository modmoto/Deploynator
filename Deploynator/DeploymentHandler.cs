using System.Collections.Generic;
using System.Linq;

namespace Deploynator
{
    public class DeploymentHandler
    {
        private readonly AzureReleaseRepository _azureReleaseRepository;
        private readonly EventBus _eventBus;
        private List<string> _deloyments;
        private List<string> _selectedDeloyments;
        private int _index;

        public DeploymentHandler(AzureReleaseRepository azureReleaseRepository, EventBus eventBus)
        {
            _azureReleaseRepository = azureReleaseRepository;
            _eventBus = eventBus;

            _eventBus.UpButtonTriggered += (_, _) => MoveU();
            _eventBus.DownButtonTriggered += (_, _) => MoveDown();
            _eventBus.SelectButtonTriggered += (_, _) => Select();
            _eventBus.DeselectButtonTriggered += (_, _) => Deselect();

            _deloyments = new List<string>
            {
                "artnerservice",
                "Messaging Service",
                "Activity Minutes service"
            };

            _selectedDeloyments = new List<string>();
            _selectedDeloyments = new List<string>();
        }

        private void Select()
        {
            _selectedDeloyments.Add(_deloyments[_index]);
            _selectedDeloyments = _selectedDeloyments.Distinct().ToList();
        }

        private void MoveDown()
        {
            if (_index <= 0) return;
            _index--;
            _eventBus.OnPreselectedDeloyment(_deloyments[_index]);
        }

        private void MoveU()
        {
            if (_index >= _deloyments.Count) return;
            _index++;
            _eventBus.OnPreselectedDeloyment(_deloyments[_index]);
        }

        private void Deselect()
        {
            _selectedDeloyments = _selectedDeloyments.Where(d => d !=_deloyments[_index]).ToList();
        }

    }

    public class AzureReleaseRepository
    {
    }
}