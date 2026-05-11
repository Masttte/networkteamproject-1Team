using Battle;
using Unity.Netcode;
using UnityEngine;

public class GeneratorPresenter : NetworkBehaviour
{
    [SerializeField] private BattleManager _battleManager;
    [SerializeField] private GeneratorView _view;
    
    public override void OnNetworkSpawn()
    {
        _battleManager.repairedGenerators.OnValueChanged += OnGeneratorValue;
        
        int initGeneratorValue = _battleManager.repairedGenerators.Value;
        int necessaryGeneratorCount = _battleManager.generatorRequiredCount;
        
        _view.NecessaryGenerator(initGeneratorValue, necessaryGeneratorCount);
    }
    
    public override void OnNetworkDespawn()
    {
        _battleManager.repairedGenerators.OnValueChanged -= OnGeneratorValue;
    }

    private void OnGeneratorValue(int previousValue, int newValue)
    {
        _view.NecessaryGenerator(newValue, _battleManager.generatorRequiredCount);
    }
}
