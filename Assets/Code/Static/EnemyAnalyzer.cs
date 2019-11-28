using UnityEngine;

// De momento lo hacemos aqui
// TODO: Moverlo a un script propio
// Ya que solo es uno a la vez vamos a hacerlo estático
public static class EnemyAnalyzer
{
    public static Transform enemyTransform;
    public static Rigidbody enemyRb;
    public static EnemyConsistency enemyConsistency;
    public static EnemyCollider enemyCollider;
    public static Vector3 estimatedToHitPosition;
    public static Targeteable targeteable;
    public static bool isActive = false;
    public static Vector3 lastEnemyPosition;

    // TODO: Ajustarlo para que trabaje con casos sin rigidbody y/o enemyconsistency
    public static void Assign(Transform enemyReference)
    {
        enemyTransform = enemyReference;
        enemyRb = enemyReference.GetComponent<Rigidbody>();
        // Chequeo extra para multipartes
        if (enemyRb == null)
            enemyRb = enemyReference.GetComponentInParent<Rigidbody>();
        // Chequeo para gusano grande. Debería ser el de la cabeza el que coja
        if (enemyRb == null)
        {
            enemyRb = enemyReference.GetComponentInChildren<Rigidbody>();
            // Para cuando cambias entre partes del cuerpo
            if (enemyRb == null)
                enemyRb = enemyReference.parent.GetComponentInChildren<Rigidbody>();
            //
            else
                enemyTransform = enemyRb.transform;
        }
        //
        enemyConsistency = enemyReference.GetComponent<EnemyConsistency>();
        // Chequeo extra para  las body parts
        if (enemyConsistency == null)
            enemyConsistency = enemyReference.GetComponentInParent<EnemyConsistency>();
        // Para el gusano grande
        if (enemyConsistency == null)
            enemyConsistency = enemyReference.parent.GetComponentInChildren<EnemyConsistency>();
        // Chequeo para los componentes que no lo tienen, como los WeakPoints
        // TODO: Ponerselos más adelante y quitar esto
        if (enemyConsistency != null)
            enemyConsistency.SetCollidersPenetrationColors();
        //
        enemyCollider = enemyReference.GetComponent<EnemyCollider>();
        //
        targeteable = enemyReference.GetComponent<Targeteable>();
        // Chequeo extra para  las body parts
        if (targeteable == null)
            targeteable = enemyReference.GetComponentInParent<Targeteable>();
        isActive = true;
    }

    public static void RecalculatePenetration()
    {
        if (enemyConsistency != null)
            enemyConsistency.SetCollidersPenetrationColors();
    }

    public static void Release()
    {
        //
        //Debug.Log("Releasing enemy");

        // Trabajamos con el transform del targeteable
        if (targeteable != null)
            lastEnemyPosition = targeteable.transform.position;
        //
        enemyTransform = null;
        enemyRb = null;

        // Chequeo para los componentes que no lo tienen, como los WeakPoints
        if (enemyConsistency != null)
            enemyConsistency.SetOriginalPenetrationColors();

        enemyConsistency = null;
        isActive = false;
    }
}