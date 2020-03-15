using System.Collections;
using UnityEngine;

public class Enemy_Basic : EnemyBase
{
    #region Initialization
    [SerializeField] [Range(1, 5)] private float movementDistance = 2f;
    public AnimationCurve movementCurve;

    private CircleCollider2D attackCollider;
    private float maxRadiusAttack;
    #endregion
    protected override EnemyState[] passivePattern => new EnemyState[]
    {
        EnemyState.Moving
    };
    protected override EnemyState[] triggeredPattern => new EnemyState[] 
    {
        EnemyState.Triggered,
        EnemyState.Action,
        EnemyState.Vulnerable
    };


    protected override EnemyBehaviour[] behaviours => new EnemyBehaviour[] 
    // je peux pas remonter la hiérarchie / je peux pas avoir accès à des fonctions créées dans les enfants
    // il faut que les refs de fonction soient dans la base donc go essayer de faire des scriptable objects qui auront chacun leur fonctionnement dedans.
    {
        new EnemyBehaviour(EnemyState.Triggered)
        
        // N'utilise pas les constructors, je ne sais pas comment utiliser le switch du CurrentBehaviour 
        // puisque je ne sais pas comment transmettre le state pour le test
    };

    protected override void Init()
    {
        attackCollider.enabled = false;
    }

    protected override void ConvertedBehaviour()
    {
        // juste un état passif où on s'assure que l'animator est au bon endroit toussa
        // (le fait de convertir se fait pas ici)
    }

    protected override void TriggeredBehaviour()
    {
        float progression = GameManager.Instance.Beat.currentBeatProgression > 0.5f ? (GameManager.Instance.Beat.currentBeatProgression - 0.5f) * 2 : 0;
        Vector2.Lerp(transformStartOfBeat.position, playerTransformStartOfBeat.position, progression);
        //bouge et arrive sur le prochain beat vers le joueur
    }

    protected override void ActionBehaviour()
    {
        attackCollider.enabled = true;
        attackCollider.radius = Mathf.Lerp(maxRadiusAttack, 0, GameManager.Instance.Beat.currentBeatProgression);
        if (GameManager.Instance.Beat.currentBeatProgression > 0.9)
        {
            attackCollider.enabled = false;
        }
        //zone dangeureuse autour de l'ennemi
    }

    protected override void MovingBehaviour()
    {
        Vector2 endOfDash = Vector2.ClampMagnitude(playerTransformStartOfBeat.position - transformStartOfBeat.position,movementDistance);
        transform.position = Vector2.Lerp(player.position, endOfDash, movementCurve.Evaluate(GameManager.Instance.Beat.currentBeatProgression * 2));
    }

    protected override void VulnerableBehaviour()
    {
        canBeDamaged = true;
        if (GameManager.Instance.Beat.currentBeatProgression > 0.9)
        {
            canBeDamaged = false;
        }
        // bouge pas et attends un coup
    }
}
