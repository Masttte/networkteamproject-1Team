using Battle;
using UnityEngine;

public class GeneratorPresenter : MonoBehaviour
{
    [SerializeField] private BattleManager _battleManager;
    [SerializeField] private GeneratorView _view;

    private void Start()
    {
        _battleManager.repairedGenerators.OnValueChanged += OnGeneratorValue;
        _battleManager.OnGameStart += OnGameStart;
    }

    private void OnDestroy()
    {
        _battleManager.repairedGenerators.OnValueChanged -= OnGeneratorValue;
        _battleManager.OnGameStart -= OnGameStart;
    }

    // StartCountdown에서 generatorRequiredCount가 확정된 이후 호출됨
    private void OnGameStart()
    {
        _view.NecessaryGenerator(_battleManager.repairedGenerators.Value, _battleManager.generatorRequiredCount.Value);
    }

    private void OnGeneratorValue(int previousValue, int newValue)
    {
        _view.NecessaryGenerator(newValue, _battleManager.generatorRequiredCount.Value);
    }
}
