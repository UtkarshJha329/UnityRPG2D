using System.Collections;
using UnityEngine;

public class MainMenuRulesAnimationScripts : MonoBehaviour
{
    private enum DirectionKey
    {
        LeftKey,
        RightKey,
        UpKey,
        DownKey
    }

    public Animator playerCharacterMovementAnimator;
    public Animator playerCharacterSwordSwingAnimator;

    public Animator leftKeyAnimator;
    public Animator rightKeyAnimator;
    public Animator upKeyAnimator;
    public Animator downKeyAnimator;

    public Animator spaceKeyAnimator;

    [SerializeField] private float waitForSecondsBeforeCyclingKeyPress = 3.5f;

    private DirectionKey currentDirectionKeyPressed = DirectionKey.LeftKey;
    private float nextSwitchKeyPressTimeAt = 0.0f;

    private string clickedParameterNameInAnimator = "clicked";
    private string isMovingParameterNameInAnimator = "isMoving";

    private string attackingUpParameterNameInAnimator = "attackingUp";
    private string attackingDownParameterNameInAnimator = "attackingDown";
    private string attackingSideParameterNameInAnimator = "attackingSide";

    private Vector3 leftKeyPlayerSpriteScale = new Vector3(-1.0f, 1.0f, 1.0f);
    private Vector3 rightKeyPlayerSpriteScale = new Vector3(1.0f, 1.0f, 1.0f);

    private void Update()
    {
        AnimateCurrentPressedKeyAndPlayerSprite();
    }

    private void AnimateCurrentPressedKeyAndPlayerSprite()
    {
        if(nextSwitchKeyPressTimeAt <= Time.time)
        {
            bool currentKeyDirectionsClicked = CurrentPressedKeyAnimator().GetBool(clickedParameterNameInAnimator);
            if (currentKeyDirectionsClicked)
            {
                CurrentPressedKeyAnimator().SetBool(clickedParameterNameInAnimator, false);
                SwitchCurrentDirectionKeyPressedToNextKey();
            }

            AnimateAttackKeyAndPlayerSprite();

            CurrentPressedKeyAnimator().SetBool(clickedParameterNameInAnimator, true);
            SetPlayerSpritesFacingDirectionBasedOnKeyPress();

            nextSwitchKeyPressTimeAt = Time.time + waitForSecondsBeforeCyclingKeyPress;
        }
    }

    private void AnimateAttackKeyAndPlayerSprite()
    {
        spaceKeyAnimator.SetBool(clickedParameterNameInAnimator, true);

        if(currentDirectionKeyPressed == DirectionKey.UpKey)
        {
            playerCharacterSwordSwingAnimator.SetBool(attackingUpParameterNameInAnimator, true);
            playerCharacterSwordSwingAnimator.SetBool(attackingDownParameterNameInAnimator, false);
            playerCharacterSwordSwingAnimator.SetBool(attackingSideParameterNameInAnimator, false);
        }
        else if(currentDirectionKeyPressed == DirectionKey.DownKey)
        {
            playerCharacterSwordSwingAnimator.SetBool(attackingUpParameterNameInAnimator, false);
            playerCharacterSwordSwingAnimator.SetBool(attackingDownParameterNameInAnimator, true);
            playerCharacterSwordSwingAnimator.SetBool(attackingSideParameterNameInAnimator, false);
        }
        else if(currentDirectionKeyPressed == DirectionKey.RightKey || currentDirectionKeyPressed == DirectionKey.LeftKey)
        {
            playerCharacterSwordSwingAnimator.SetBool(attackingUpParameterNameInAnimator, false);
            playerCharacterSwordSwingAnimator.SetBool(attackingDownParameterNameInAnimator, false);
            playerCharacterSwordSwingAnimator.SetBool(attackingSideParameterNameInAnimator, true);
        }
    }

    private void SetPlayerSpritesFacingDirectionBasedOnKeyPress()
    {
        if (currentDirectionKeyPressed == DirectionKey.LeftKey)
        {
            playerCharacterMovementAnimator.transform.localScale = leftKeyPlayerSpriteScale;
            playerCharacterSwordSwingAnimator.transform.localScale = leftKeyPlayerSpriteScale;
        }
        else if (currentDirectionKeyPressed == DirectionKey.RightKey)
        {
            playerCharacterMovementAnimator.transform.localScale = rightKeyPlayerSpriteScale;
            playerCharacterSwordSwingAnimator.transform.localScale = rightKeyPlayerSpriteScale;
        }
    }

    private void SwitchCurrentDirectionKeyPressedToNextKey()
    {
        if (currentDirectionKeyPressed == DirectionKey.LeftKey)
        {
            currentDirectionKeyPressed = DirectionKey.UpKey;
        }
        else if (currentDirectionKeyPressed == DirectionKey.RightKey)
        {
            currentDirectionKeyPressed = DirectionKey.DownKey;
        }
        else if (currentDirectionKeyPressed == DirectionKey.UpKey)
        {
            currentDirectionKeyPressed = DirectionKey.RightKey;
        }
        else if (currentDirectionKeyPressed == DirectionKey.DownKey)
        {
            currentDirectionKeyPressed = DirectionKey.LeftKey;
        }
    }

    private Animator CurrentPressedKeyAnimator()
    {
        if(currentDirectionKeyPressed == DirectionKey.LeftKey)
        {
            return leftKeyAnimator;
        }
        else if(currentDirectionKeyPressed == DirectionKey.RightKey)
        {
            return rightKeyAnimator;
        }
        else if(currentDirectionKeyPressed == DirectionKey.UpKey)
        {
            return upKeyAnimator;
        }
        else if(currentDirectionKeyPressed == DirectionKey.DownKey)
        {
            return downKeyAnimator;
        }
        else
        {
            return null;
        }
    }
}
