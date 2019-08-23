using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;
using System.Linq;

namespace Qitz.Input
{
    public class InputProvider : MonoBehaviour, IInputProvider
    {
        const int LONG_PRESS_INTERVAL = 4;
        ReactiveProperty<bool> aButton = new BoolReactiveProperty();
        ReactiveProperty<bool> wButton = new BoolReactiveProperty();
        ReactiveProperty<bool> sButton = new BoolReactiveProperty();
        ReactiveProperty<bool> eButton = new BoolReactiveProperty();
        ReactiveProperty<bool> dButton = new BoolReactiveProperty();
        ReactiveProperty<bool> rButton = new BoolReactiveProperty();

        Subject<bool> leftSubject = new Subject<bool>();
        Subject<bool> rightSubject = new Subject<bool>();
        Subject<bool> upSubject = new Subject<bool>();
        Subject<bool> downSubject = new Subject<bool>();

        ReactiveProperty<bool> startButton = new BoolReactiveProperty();
        ReactiveProperty<INesicaUserData> nesicaInput = new ReactiveProperty<INesicaUserData>(new NesicaInputData(false));

        public IReadOnlyReactiveProperty<bool> AButton { get { return aButton; } }
        public IReadOnlyReactiveProperty<bool> WButton { get { return wButton; } }
        public IReadOnlyReactiveProperty<bool> SButton { get { return sButton; } }
        public IReadOnlyReactiveProperty<bool> EButton { get { return eButton; } }
        public IReadOnlyReactiveProperty<bool> DButton { get { return dButton; } }
        public IReadOnlyReactiveProperty<bool> RButton { get { return rButton; } }

        public IObservable<bool> AnyButtonPress => AButton.Merge(WButton).Merge(SButton).Merge(EButton).Merge(DButton).Merge(RButton);
        public IObservable<bool> AnyKeyPress => AnyButtonPress.Merge(ArrowLeftButton).Merge(ArrowRightButton).Merge(ArrowUpButton).Merge(ArrowDownButton);

        //======================================================

        public IReadOnlyReactiveProperty<bool> ArrowLeftButton { get { return leftSubject.ToReactiveProperty(); } }
        public IReadOnlyReactiveProperty<bool> ArrowRightButton { get { return rightSubject.ToReactiveProperty(); } }
        public IReadOnlyReactiveProperty<bool> ArrowUpButton { get { return upSubject.ToReactiveProperty(); } }
        public IReadOnlyReactiveProperty<bool> ArrowDownButton { get { return downSubject.ToReactiveProperty(); } }

        public ISubject<bool> LongPressArrowLeftButtonSubject { get { return leftSubject; } }
        public ISubject<bool> LongPressArrowRightButtonSubject { get { return rightSubject; } }
        public ISubject<bool> LongPressArrowUpButtonSubject { get { return upSubject; } }
        public ISubject<bool> LongPressArrowDownButtonSubject { get { return downSubject; } }

        //======================================================
        public IReadOnlyReactiveProperty<bool> StartButton { get { return startButton; } }
        //======================================================

        void Awake()
        {
            //Debug.LogError("InputProviderAwake!!");
            //A
            this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.A)).DistinctUntilChanged().Subscribe(inputA =>
            {
                //Debug.Log("inputA!!");
                aButton.Value = inputA;
            }).AddTo(this.gameObject);
            //W
            this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.W)).DistinctUntilChanged().Subscribe(inputW =>
            {
                wButton.Value = inputW;
            }).AddTo(this.gameObject);
            //S
            this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.S)).DistinctUntilChanged().Subscribe(inputS =>
            {
                sButton.Value = inputS;
            }).AddTo(this.gameObject);
            //E
            this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.E)).DistinctUntilChanged().Subscribe(inputE =>
            {
                eButton.Value = inputE;
            }).AddTo(this.gameObject);
            //D
            this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.D)).DistinctUntilChanged().Subscribe(inputD =>
            {
                dButton.Value = inputD;
            }).AddTo(this.gameObject);
            //R
            this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.R)).DistinctUntilChanged().Subscribe(inputR =>
            {
                rButton.Value = inputR;
            }).AddTo(this.gameObject);

            //=================================================================================

            //Left

            var leftObservable = this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.LeftArrow)).DistinctUntilChanged();
            var leftLongPressObservable = this.UpdateAsObservable()
                                            .Select(_ => Input.GetKey(KeyCode.LeftArrow))
                                            .Where(onKeyPressed => onKeyPressed);
            var delayLeftLongPressStream = this.UpdateAsObservable()
                                            .SkipUntil(leftLongPressObservable.DelayFrame(LONG_PRESS_INTERVAL)) //タップされてから指定フレーム経過したら本流を通す
                                            .ThrottleFirstFrame(LONG_PRESS_INTERVAL-1)
                                            .Select(_ => Input.GetKey(KeyCode.LeftArrow))
                                            .Where(onKeyPressed => onKeyPressed);

            Observable.Merge(leftObservable, delayLeftLongPressStream).Subscribe(input =>
            {
                leftSubject.OnNext(input);
            }).AddTo(this.gameObject);


            //Right
            var rightObservable = this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.RightArrow)).DistinctUntilChanged();
            var rightLongPressObservable = this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.RightArrow)).Where(onKeyPressed => onKeyPressed);
            var delayRightLongPressStream = this.UpdateAsObservable()
                                .SkipUntil(rightLongPressObservable.DelayFrame(LONG_PRESS_INTERVAL)) //タップされてから指定フレーム経過したら本流を通す
                                .ThrottleFirstFrame(LONG_PRESS_INTERVAL - 1)
                                .Select(_ => Input.GetKey(KeyCode.RightArrow))
                                .Where(onKeyPressed => onKeyPressed);

            Observable.Merge(rightObservable, delayRightLongPressStream).Subscribe(input =>
            {
                rightSubject.OnNext(input);
            }).AddTo(this.gameObject);
            //Up
            var upObservable = this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.UpArrow)).DistinctUntilChanged();
            var upLongPressObservable = this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.UpArrow)).Where(onKeyPressed => onKeyPressed);
            var delayUpLongPressStream = this.UpdateAsObservable()
                    .SkipUntil(upLongPressObservable.DelayFrame(LONG_PRESS_INTERVAL)) //タップされてから指定フレーム経過したら本流を通す
                    .ThrottleFirstFrame(LONG_PRESS_INTERVAL - 1)
                    .Select(_ => Input.GetKey(KeyCode.UpArrow))
                    .Where(onKeyPressed => onKeyPressed);

            Observable.Merge(upObservable, delayUpLongPressStream).Subscribe(input =>
            {
                upSubject.OnNext(input);
            }).AddTo(this.gameObject);

            //Down
            var downObservable = this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.DownArrow)).DistinctUntilChanged();
            var downLongPressObservable = this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.DownArrow)).Where(onKeyPressed => onKeyPressed);
            var delayDownLongPressStream = this.UpdateAsObservable()
                    .SkipUntil(downLongPressObservable.DelayFrame(LONG_PRESS_INTERVAL)) //タップされてから指定フレーム経過したら本流を通す
                    .ThrottleFirstFrame(LONG_PRESS_INTERVAL - 1)
                    .Select(_ => Input.GetKey(KeyCode.DownArrow))
                    .Where(onKeyPressed => onKeyPressed);

            Observable.Merge(downObservable, delayDownLongPressStream).Subscribe(input =>
            {
                downSubject.OnNext(input);
            }).AddTo(this.gameObject);

            //===================================================================================

            //Space
            this.UpdateAsObservable().Select(_ => Input.GetKey(KeyCode.Space)).DistinctUntilChanged().Subscribe(inputSpace =>
            {
                startButton.Value = inputSpace;
            }).AddTo(this.gameObject);


            //InputProviderのクラスをシーン中に重複させたく無いのでここからは、重複排除処理
            InputProvider instance = UnityEngine.Object.FindObjectOfType<InputProvider>();
            if (instance != this)
            {
                Destroy(this);
                Debug.LogError("シーン中に他のInputProviderが重複しているようです。");
            }

        }

        void OnDestroy()
        {
            //InputProviderを破棄する。
            this.DisposeProvider();
        }
    }

    public interface IInputProvider
    {
        IReadOnlyReactiveProperty<bool> AButton { get; }
        IReadOnlyReactiveProperty<bool> WButton { get; }
        IReadOnlyReactiveProperty<bool> SButton { get; }
        IReadOnlyReactiveProperty<bool> EButton { get; }
        IReadOnlyReactiveProperty<bool> DButton { get; }
        IReadOnlyReactiveProperty<bool> RButton { get; }
        //======================================================

        IReadOnlyReactiveProperty<bool> ArrowLeftButton { get; }
        ISubject<bool> LongPressArrowLeftButtonSubject { get; }
        IReadOnlyReactiveProperty<bool> ArrowRightButton { get; }
        ISubject<bool> LongPressArrowRightButtonSubject { get; }
        IReadOnlyReactiveProperty<bool> ArrowUpButton { get; }
        ISubject<bool> LongPressArrowUpButtonSubject { get; }
        IReadOnlyReactiveProperty<bool> ArrowDownButton { get; }
        ISubject<bool> LongPressArrowDownButtonSubject { get; }

        //======================================================
        IReadOnlyReactiveProperty<bool> StartButton { get; }
        //======================================================
        //======================================================
        IObservable<bool> AnyButtonPress { get; }
        IObservable<bool> AnyKeyPress { get; }
    }

    //IRecivableInputを実装したクラスは GetInputProvider()でInputProviderを取得できる。
    public interface IReceivableInput
    {
    }
    public static class InputExtentions
    {
        static IInputProvider iInputProvider;
        public static IInputProvider GetInputProvider(this IReceivableInput recivableInput)
        {
            if (iInputProvider == null)
            {
                iInputProvider = UnityEngine.Object.FindObjectOfType<InputProvider>();
                return UnityEngine.Object.FindObjectOfType<InputProvider>();
            }
            return iInputProvider;
        }
        public static void DisposeProvider(this IInputProvider _iInputProvider)
        {
            iInputProvider = null;
        }
    }

    public interface INesicaUserData: IRecordWriteAble
    {
        //カードが乗っているかどうか？
        bool OnCard { get; }
        //ユーザー名
        string UserName { get; }
        //一人プレイのベストスコア
        int SinglePlayBestScore { get; }
        //VSプレイのベストスコア
        int VsPlayBestScore { get; }
        //現在いるお店の名前
        string CurrentShopName { get; }
        //ゲームのプレイ履歴データ
        List<IGamePlayRecord> RecordList { get; }
        //月の通算勝利数
        int MonthVictoryCount { get; }
        //通算勝利数
        int TotalVictoryCount { get; }
        //対戦レート
        int Ratio { get; }
        // 連勝数
        int ConsecutiveCount { get; }
    }

    public interface IGamePlayRecord
    {
        long PlayTimeStamp { get; }
        int Score { get; }
        string ShopName { get; }
        LevelsGameMode GameMode { get; }
        GameResult GameResult { get; }
    }



    public enum LevelsGameMode
    {
        NONE,
        SINGLE,
        VS,
    }
    public enum GameResult
    {
        NONE,
        WIN,
        LOSE,
    }

    
}

