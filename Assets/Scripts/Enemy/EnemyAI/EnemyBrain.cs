using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    //敵が持っている行動
    private readonly List<IEnemyAction> actions = new();
    //現在実行している行動
    private IEnemyAction currentAction;
    //行動判定の間隔
    [SerializeField] private float decisionInterval = 0.2f;

    private float timer;

    //行動を登録する
    public void RegisterAction(IEnemyAction action)
    {
        if (action == null) { return; }
        if (actions.Contains(action)) { return; }

        actions.Add(action);

        //優先順位が高い順に入れ替え
        actions.Sort((a, b) =>
        b.Priolity.CompareTo(a.Priolity));
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = Mathf.Max(0.01f, decisionInterval);
            SelectAction();
        }

        //現在の行動を実行 
        currentAction?.Execute();
    }

    private void SelectAction()
    {
        IEnemyAction nextaction = null;

        //優先順位が高い行動から確認
        foreach(IEnemyAction action in actions)
        {
            if (action.CanExecute())
            {
                nextaction = action;
                break;
            }
        }

        //行動が変わらなければ何もいない
        if (currentAction == nextaction) { return; }
        //現在の行動を終了
        currentAction?.Exit();
        //新しい行動に切り替える
        currentAction = nextaction;
        //新しい行動を開始
        currentAction?.Enter();
    }

    private void OnDisable()
    {
        currentAction?.Exit();
        currentAction = null;
        timer = 0f;
    }


}
