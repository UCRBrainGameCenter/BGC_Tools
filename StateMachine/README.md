# State Machine

This state machine has been implemented as a generic solution to implementing state machines in C# with Unity. It is also a response to a previously tested method with using the [Unity Animation State Machine](https://docs.unity3d.com/Manual/AnimationStateMachines.html) which while powerful and [visual](https://www.google.com/search?q=unity+animation+state+machine&client=firefox-b-1-d&source=lnms&tbm=isch&sa=X&ved=0ahUKEwj5_Ljl7NLhAhVZIDQIHRu6AlUQ_AUIDigB&biw=1920&bih=983#imgrc=ap47IpO2-J87lM:), it is frame capped. We do, however, implement similar functionality to the animation state machine.

## Trigger and Bool Transitions

A state can be transitioned through either triggers or bools. A trigger is a boolean that once used by a state to transition will be consumed. Meaning that if I have three states connected that used the same trigger condition to transiiton, I would have to activate the trigger each time to transition. In this same situation, where they require a bool instead of a trigger, though, all three states will transition in the same frame.

## States

Every state will implement the `State` class which provides `OnStateEnter` and `OnStateExit` as functions that must be overrode. They can also implement an `Update` function. For this to be called, the state machine must be called in a `MonoBehaviour` class `Update` call. 

## Extra Notes

The example provided below is very simplistic and only to show a basic use case. More complex use cases will typically have states take in arguments as a way to handle game state. As an example, in several of our own games we use this state machine to implement the entire game loop and menu management. 

## Example

### Code

```c#
using UnityEngine;
using BGC.StateMachine;

public class EntryState : State
{
    protected override string DefaultName => "EntryState";

    protected override void OnStateEnter()
    {
        ActivateTrigger("nextstate");
    }

    // pass, ignore the bad styling for the sake of brevity
    protected override void OnStateExit() {}
}

public class RandomPrintState : State
{
    protected override string DefaultName => "RandomPrint";

    protected override void OnStateEnter()
    {
        if (Random.Range(0f, 1f) >= 0.5f)
        {
            Debug.Log("I am greater than or equal to 0.5f");
        }
        else
        {
            Debug.Log("I am less than 0.5f");
        }

        ActivateTrigger("nextstate");
    }

    // pass, ignore the bad styling for the sake of brevity
    protected override void OnStateExit() {}
}

public class HelloWorldState : State
{
    protected override string DefaultName => "Hello world";

    protected override void OnStateEnter()
    {
        Debug.Log("hello world");

        // 50/50 chance that the bool is set to true or false which defines
        // which state we transition to next
        SetBool("nextstate", Random.Range(0f, 1f) >= 0.5f);
    }

    // pass, ignore the bad styling for the sake of brevity
    protected override void OnStateExit() {}
}

public class ExitState : State
{
    protected override string DefaultName => "Exit State";

    protected override void OnStateEnter() {}
    protected override void OnStateExit() {}
}

public class ExampleStateMachineImplementation : MonoBehaviour
{
    private void Start()
    {
        StateMachine stateMachine = new StateMachine(verbose: true);

        EntryState entryState = new EntryState();
        HelloWorldState helloWorldState = new HelloWorldState();
        RandomPrintState randomPrintState = new RandomPrintState();
        ExitState exitState = new ExitState();

        stateMachine.AddEntryState(entryState);
        stateMachine.AddState(helloWorldState);
        stateMachine.AddState(randomPrintState);
        stateMachine.AddState(exitState);

        stateMachine.AddTransition(
            entryState,
            helloWorldState,
            new TriggerCondition("nextstate"));

        stateMachine.AddTransition(
            helloWorldState,
            randomPrintState,
            new BoolCondition("nextstate", true));

        stateMachine.AddTransition(
            helloWorldState,
            exitState,
            new BoolCondition("nextstate", false));

        stateMachine.AddTransition(
            randomPrintState,
            exitState,
            new TriggerCondition("nextstate"));

        stateMachine.Start();
    }
}

```

### Output

Please note that the output is the output from when the state machine has verbose set to true. If verbose was set to false, then the log lines with "entered" or "left" would not be logged.

#### 1

This is the output when `RandomPrintState` is not entered.

```
EntryState entered.
EntryState left.
Hello world entered.
hello world
Hello world left.
Exit State entered.
```

#### 2

This is the output when `RandomPrintState` is entered

```
EntryState entered.
EntryState left.
Hello world entered.
hello world
Hello world left.
RandomPrint entered.
I am greater than or equal to 0.5f
RandomPrint left.
Exit State entered.
```