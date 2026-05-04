// using UnityEngine;

// public class BossLaserBeam : MonoBehaviour
// {
//     [SerializeField] private LineRenderer line;
//     [SerializeField] private float laserWidth = 0.35f;
//     [SerializeField] private LayerMask playerMask;

//     private BossController boss;
//     private Transform origin;
//     private Vector3 currentDirection;
//     private float duration;
//     private float rotateDegrees;
//     private float damagePerSecond;
//     private LayerMask wallMask;
//     private float timer;

//     public void Initialize(
//         BossController bossController,
//         Transform laserOrigin,
//         Vector3 startDirection,
//         float laserDuration,
//         float totalRotateDegrees,
//         float dps,
//         LayerMask walls
//     )
//     {
//         boss = bossController;
//         origin = laserOrigin;
//         currentDirection = startDirection.normalized;
//         duration = laserDuration;
//         rotateDegrees = totalRotateDegrees;
//         damagePerSecond = dps;
//         wallMask = walls;

//         if (line == null)
//             line = GetComponent<LineRenderer>();

//         if (line != null)
//         {
//             line.positionCount = 2;
//             line.startWidth = laserWidth;
//             line.endWidth = laserWidth;
//         }
//     }

//     private void Update()
//     {
//         if (boss == null || boss.IsDead || origin == null)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         timer += Time.deltaTime;

//         FollowPlayerSide();
//         UpdateLaserVisual();
//         DamagePlayerIfHit();

//         if (timer >= duration)
//             Destroy(gameObject);
//     }

//     private void FollowPlayerSide()
//     {
//         if (boss.Target == null)
//             return;

//         Vector3 toPlayer = boss.Target.position - origin.position;
//         toPlayer.y = 0f;

//         if (toPlayer.sqrMagnitude < 0.001f)
//             return;

//         Vector3 desiredDirection = toPlayer.normalized;

//         float maxRotateThisFrame = (rotateDegrees / duration) * Time.deltaTime;

//         currentDirection = Vector3.RotateTowards(
//             currentDirection,
//             desiredDirection,
//             maxRotateThisFrame * Mathf.Deg2Rad,
//             0f
//         );
//     }

//     private void UpdateLaserVisual()
//     {
//         Vector3 start = origin.position;
//         Vector3 end = start + currentDirection * 100f;

//         if (Physics.Raycast(start, currentDirection, out RaycastHit wallHit, 100f, wallMask))
//         {
//             end = wallHit.point;
//         }

//         transform.position = start;
//         transform.rotation = Quaternion.LookRotation(currentDirection);

//         if (line != null)
//         {
//             line.SetPosition(0, start);
//             line.SetPosition(1, end);
//         }
//     }

//     private void DamagePlayerIfHit()
//     {
//         Vector3 start = origin.position;

//         if (Physics.SphereCast(start, laserWidth, currentDirection, out RaycastHit hit, 100f, playerMask))
//         {
//             PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();

//             if (playerHealth == null)
//                 playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();

//             if (playerHealth != null)
//                 playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
//         }
//     }
// }

using UnityEngine;

public class BossLaserBeam : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private float laserWidth = 0.35f;
    [SerializeField] private LayerMask playerMask;

    private BossController boss;
    private Transform origin;

    private Vector3 startDirection;
    private Vector3 currentDirection;

    private float duration;
    private float rotateDegrees;
    private float damagePerSecond;
    private LayerMask wallMask;

    private float timer;
    private float sweepSign = 1f;

    public void Initialize(
        BossController bossController,
        Transform laserOrigin,
        Vector3 startDir,
        float laserDuration,
        float totalRotateDegrees,
        float dps,
        LayerMask walls
    )
    {
        boss = bossController;
        origin = laserOrigin;

        startDirection = startDir.normalized;
        currentDirection = startDirection;

        duration = laserDuration;
        rotateDegrees = totalRotateDegrees;
        damagePerSecond = dps;
        wallMask = walls;

        timer = 0f;

        sweepSign = Random.value < 0.5f ? -1f : 1f;

        if (line == null)
            line = GetComponent<LineRenderer>();

        if (line != null)
        {
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = laserWidth;
            line.endWidth = laserWidth;
        }
    }

    private void Update()
    {
        if (boss == null || boss.IsDead || origin == null)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;

        SweepLaser();
        UpdateLaserVisual();
        DamagePlayerIfHit();

        if (timer >= duration)
            Destroy(gameObject);
    }

    private void SweepLaser()
    {
        float progress = Mathf.Clamp01(timer / duration);
        float angle = rotateDegrees * progress * sweepSign;

        currentDirection = Quaternion.AngleAxis(angle, Vector3.up) * startDirection;
    }

    private void UpdateLaserVisual()
    {
        Vector3 start = origin.position;
        Vector3 end = start + currentDirection * 100f;

        if (Physics.Raycast(start, currentDirection, out RaycastHit wallHit, 100f, wallMask))
            end = wallHit.point;

        transform.position = start;
        transform.rotation = Quaternion.LookRotation(currentDirection);

        if (line != null)
        {
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }
    }

    private void DamagePlayerIfHit()
    {
        Vector3 start = origin.position;

        if (Physics.SphereCast(start, laserWidth, currentDirection, out RaycastHit hit, 100f, playerMask))
        {
            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}