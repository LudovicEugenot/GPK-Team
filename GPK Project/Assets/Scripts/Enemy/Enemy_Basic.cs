using System.Collections;
using UnityEngine;

public class Enemy_Basic : EnemyBase
{
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
    {
        new EnemyBehaviour(EnemyState.Triggered) // N'utilise pas les constructors, je ne sais pas comment utiliser le switch du CurrentBehaviour puisque je ne sais pas comment transmettre le state
        // pour le test
    };



    protected override void Init()
    {

    }

    protected override void ConvertedBehaviour()
    {
        // juste un état passif où on s'assure que l'animator est au bon endroit toussa
        // (le fait de convertir se fait pas ici)
    }

    protected override void TriggeredBehaviour()
    {
        //bouge et arrive sur le prochain beat vers le joueur
    }

    protected override void ActionBehaviour()
    {
        //zone dangeureuse autour de l'ennemi
    }

    protected override void MovingBehaviour()
    {

        //se déplace chaque beat vers le joueur
    }

    protected override void VulnerableBehaviour()
    {
        canBeDamaged = true;
        // bouge pas et attends un coup
    }
}
