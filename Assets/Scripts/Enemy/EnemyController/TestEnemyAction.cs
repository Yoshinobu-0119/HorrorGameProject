
using UnityEngine;

public class TestEnemyAction : IEnemyAction
{
    public int Priority { get; private set; }

    private string actionName;
    private bool canExecute;

    public TestEnemyAction(string name, int priority)
    {
        actionName = name;
        Priority = priority;
    }

    // 行動が可能かどうか
    public bool CanExecute()
    {
        return canExecute;
    }

    // 外部から実行条件を変更
    public void SetExecutable(bool value)
    {
        canExecute = value;
    }

    // 行動開始
    public void Enter()
    {
        Debug.Log(actionName + " 開始");
    }

    // 行動中
    public void Execute()
    {
        // 今回は何もしない
    }

    // 行動終了
    public void Exit()
    {
        Debug.Log(actionName + " 終了");
    }
}
