/*// Letterbox/Pillarbox automatico per un aspect fisso
[RequireComponent(typeof(Camera))]
public class FixedAspect : MonoBehaviour
{
    [SerializeField] Vector2 targetAspect = new Vector2(16, 9); // o 320:180 ecc.

    void Start() => Apply();

#if UNITY_EDITOR
    // così lo vedi live se ridimensioni la finestra
    void Update() { #if UNITY_EDITOR
        if (!Application.isPlaying) Apply(); #endif }
#endif

    void Apply()
    {
        var cam = GetComponent<Camera>();
        float desired = targetAspect.x / targetAspect.y;
        float window  = (float)Screen.width / Screen.height;

        if (Mathf.Approximately(window, desired))
        {
            cam.rect = new Rect(0, 0, 1, 1);
            return;
        }

        if (window > desired) // finestra più larga ⇒ bande verticali
        {
            float w = desired / window;
            float x = (1 - w) * 0.5f;
            cam.rect = new Rect(x, 0, w, 1);
        }
        else                  // finestra più stretta ⇒ bande orizzontali
        {
            float h = window / desired;
            float y = (1 - h) * 0.5f;
            cam.rect = new Rect(0, y, 1, h);
        }
    }
}*/