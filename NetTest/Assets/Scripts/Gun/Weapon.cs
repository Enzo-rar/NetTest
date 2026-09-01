using UnityEngine;
using Fusion;
public class Weapon : MonoBehaviour
{
    public enum WeaponType { Hitscan, Projectile }
    public enum FireMode { Automatic, SemiAutomatic }

    [Header("Atributos Base")]
    public string weaponName = "Pistola Base";
    public WeaponType type = WeaponType.Hitscan;
    public FireMode fireMode = FireMode.SemiAutomatic;

    public int maxAmmo = 15;
    private int currentAmmo;

    public float fireRate = 0.2f; // Segundos entre disparos
    public float damage = 25f;

    [Header("Disparo Múltiple (Ej: Escopetas)")]
    public int projectilesPerShot = 1;
    public float spreadAngle = 0f; // Dispersión de las balas

    [Header("Referencias")]
    public Transform shootPoint; // De dónde sale la bala visualmente
    public GameObject projectilePrefab; // Solo si es WeaponType.Projectile

    private float nextTimeToFire = 0f;
    private bool wasFiring = false;

    // Nuestro componente de disparo secundario
    private AltFireBehavior altFireMethod;

    void Start()
    {
        currentAmmo = maxAmmo;
        AssignRandomAltFire();
    }

    /// <summary>
    /// Esta función simula la generación aleatoria del disparo secundario.
    /// Al hacer spawn el arma, elige un componente al azar y se lo pega.
    /// </summary>
    private void AssignRandomAltFire()
    {
        // Aquí en el futuro puedes hacer un array de Tipos y elegir uno al azar con Random.Range.
        // Por ahora, forzamos uno de prueba para que veas la arquitectura.
        altFireMethod = gameObject.AddComponent<AltFireBurst>();
        altFireMethod.Initialize(this);
    }

    /// <summary>
    /// Llamado desde el script que controla al jugador cada frame que tenga el arma equipada.
    /// </summary>
    public void HandleWeaponInputs(NetworkInputData input, Transform cameraPoint)
    {
        // 1. Lógica de disparo principal
        bool triesToFire = input.Fire;

        if (triesToFire && Time.time >= nextTimeToFire)
        {
            if (fireMode == FireMode.Automatic || (fireMode == FireMode.SemiAutomatic && !wasFiring))
            {
                Shoot(cameraPoint);
                nextTimeToFire = Time.time + fireRate;
            }
        }

        wasFiring = triesToFire;

    }

    private void Shoot(Transform cameraPoint)
    {
        if (currentAmmo <= 0) return;

        currentAmmo--;
        Debug.Log($"[Arma] ¡Bang! Munición: {currentAmmo}/{maxAmmo}");

        for (int i = 0; i < projectilesPerShot; i++)
        {
            Vector3 shootDirection = GetSpreadDirection(cameraPoint.forward);

            if (type == WeaponType.Hitscan)
            {
                PerformHitscan(cameraPoint.position, shootDirection);
            }
            else
            {
                SpawnProjectile(cameraPoint.position, shootDirection);
            }
        }

        // Comprobar si nos hemos quedado sin balas tras el disparo
        if (currentAmmo <= 0)
        {
            DiscardWeapon();
        }
    }

    private Vector3 GetSpreadDirection(Vector3 forward)
    {
        if (spreadAngle == 0) return forward;

        // Añade una ligera rotación aleatoria para simular dispersión
        float angleX = Random.Range(-spreadAngle, spreadAngle);
        float angleY = Random.Range(-spreadAngle, spreadAngle);

        Quaternion spreadRotation = Quaternion.Euler(angleX, angleY, 0);
        return spreadRotation * forward;
    }

    private void PerformHitscan(Vector3 origin, Vector3 direction)
    {
        // En el futuro, aquí registrarás el hit en tu CSVMetricsLogger si impacta al host
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f))
        {
            Debug.Log($"Hitscan impactó a: {hit.collider.name}");
        }
    }

    private void SpawnProjectile(Vector3 origin, Vector3 direction)
    {
        if (projectilePrefab != null)
        {
            // Instanciar proyectil físico
            GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(direction));
            // Deberá tener un script propio que aplique fuerza al Rigidbody
        }
    }

    private void DiscardWeapon()
    {
        Debug.Log("<color=red>[Arma]</color> Sin munición. Despawning arma...");
        // Más adelante puedes instanciar un prefab físico que salga volando hacia delante (rb.AddForce)

        // Por ahora, destruimos el arma
        Destroy(gameObject);

        // Importante: Tendrás que avisar al jugador de que ya no tiene arma en las manos
    }
}