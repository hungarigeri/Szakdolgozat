using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // A Player objektum, amit követni fogunk.
    // Ezt az Inspector ablakban kell beállítani.
    public Transform target;

    // A kamera pozíciója a targethez képest.
    // Top-down játékoknál gyakran csak a Z tengelyt érdemes -10-en hagyni.
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    
    // A követés simasága (minél nagyobb a szám, annál lassabb/simább a követés).
    // 0 felett bármi működik, 10 egy jó alapérték.
    public float smoothSpeed = 10f;

    // A LateUpdate a legideálisabb a kamera mozgásához, 
    // mert a célpont (Player) már befejezte a mozgását az Update-ben és FixedUpdate-ben.
    void LateUpdate()
    {
        // 1. Célpozíció (Desired Position) Kiszámítása
        // A Player pozíciója + a beállított offset (eltolás)
        Vector3 desiredPosition = target.position + offset;

        // 2. Simított Pozíció (Smoothed Position) Kiszámítása
        // Lerp (Lineáris interpoláció) használata: simán mozgatja a kamera aktuális pozícióját 
        // a célpozíció felé. Time.deltaTime teszi képkocka-függetlenné a simítást.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. A Kamera Pozíciójának Beállítása
        transform.position = smoothedPosition;

        // (Opcionális: Ha szeretnéd, hogy a kamera a Player felé nézzen)
        // transform.LookAt(target);
    }
}