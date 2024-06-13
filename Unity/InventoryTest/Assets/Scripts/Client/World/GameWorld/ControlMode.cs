
using UnityEngine; 
public class ControlMode
{
    public enum ControlState
    {
        Character,
        UI
    }

    ControlState state = ControlState.Character;
    public ControlState CurrentState { get => state; }   

    public void SetState(ControlState state)
    {
        this.state = state;
        switch (this.state)
        {
            case ControlState.Character:
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case ControlState.UI:
                Cursor.lockState = CursorLockMode.Confined;
                break;
        }
    }
}