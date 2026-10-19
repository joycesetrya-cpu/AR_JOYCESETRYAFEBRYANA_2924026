using UnityEngine;

public class JoystickPlayer : MonoBehaviour
{
    public float speed = 5f;
    [Range(0.08f, 0.2f)] public float ukuranBtn = 0.13f;   // rasio terhadap sisi layar terpendek
    public string targetName = "Target";
    public string judul = "AR Praktikum P05/P07";
    public string pesanSukses = "Target tersentuh. Praktikum berhasil.";

    Rigidbody rb;
    Texture2D texBulat, texRing, texPanah, texPanel;
    GUIStyle panelStyle, notifStyle;
    Rect rUp, rDown, rLeft, rRight;
    bool up, down, left, right;
    bool success;
    float btnSize, minSide;

    // ------------------------------------------------------------
    // Pembuatan tekstur (semua digambar lewat kode)
    // ------------------------------------------------------------
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        texBulat = BuatBentuk(128, 64f, 0f);    // lingkaran penuh
        texRing  = BuatBentuk(128, 64f, 4f);    // garis tepi lingkaran
        texPanel = BuatBentuk(64, 20f, 0f);     // kotak sudut membulat
        texPanah = BuatPanah();
    }

    // radius = sudut membulat (size/2 = lingkaran), ring > 0 = hanya garis tepi
    Texture2D BuatBentuk(int size, float radius, float ring)
    {
        var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        float half = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float px = Mathf.Abs(x + 0.5f - half) - (half - radius);
                float py = Mathf.Abs(y + 0.5f - half) - (half - radius);
                float d = new Vector2(Mathf.Max(px, 0), Mathf.Max(py, 0)).magnitude
                          + Mathf.Min(Mathf.Max(px, py), 0) - radius;     // d < 0 = di dalam
                float a = Mathf.Clamp01(0.5f - d);
                if (ring > 0) a *= Mathf.Clamp01(d + ring + 0.5f);
                t.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        t.Apply();
        return t;
    }

    Texture2D BuatPanah()
    {
        int n = 128;
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        Vector2 A = new Vector2(64, 104), B = new Vector2(26, 34), C = new Vector2(102, 34);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < 4; sy++)
                    for (int sx = 0; sx < 4; sx++)
                    {
                        Vector2 p = new Vector2(x + (sx + 0.5f) / 4f, y + (sy + 0.5f) / 4f);
                        if (DalamSegitiga(p, A, B, C)) hit++;
                    }
                t.SetPixel(x, y, new Color(1, 1, 1, hit / 16f));
            }
        t.Apply();
        return t;
    }

    static float Sisi(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    static bool DalamSegitiga(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sisi(p, a, b), d2 = Sisi(p, b, c), d3 = Sisi(p, c, a);
        bool neg = d1 < 0 || d2 < 0 || d3 < 0;
        bool pos = d1 > 0 || d2 > 0 || d3 > 0;
        return !(neg && pos);
    }

    // ------------------------------------------------------------
    // Layout & input
    // ------------------------------------------------------------
    void HitungLayout()
    {
        minSide = Mathf.Min(Screen.width, Screen.height);
        btnSize = minSide * ukuranBtn;

        float s = btnSize;
        float g = s * 0.12f;
        float margin = minSide * 0.06f;
        float cx = Screen.width / 2f;
        float top = Screen.height - margin - (3 * s + 2 * g);
        float mid = top + s + g;

        rUp    = new Rect(cx - s / 2,        top,         s, s);
        rLeft  = new Rect(cx - s * 1.5f - g, mid,         s, s);
        rRight = new Rect(cx + s / 2 + g,    mid,         s, s);
        rDown  = new Rect(cx - s / 2,        mid + s + g, s, s);
    }

    void Update()
    {
        HitungLayout();
        up = down = left = right = false;

        // semua sentuhan (HP) agar bisa tekan 2 tombol sekaligus
        for (int i = 0; i < Input.touchCount; i++)
            CekTitik(Input.GetTouch(i).position);

        // mouse (editor)
        if (Input.GetMouseButton(0))
            CekTitik(Input.mousePosition);

        // keyboard (untuk tes di editor)
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) up = true;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) down = true;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) left = true;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) right = true;
    }

    void CekTitik(Vector2 screenPos)
    {
        Vector2 p = new Vector2(screenPos.x, Screen.height - screenPos.y); // ke koordinat GUI
        if (rUp.Contains(p)) up = true;
        if (rDown.Contains(p)) down = true;
        if (rLeft.Contains(p)) left = true;
        if (rRight.Contains(p)) right = true;
    }

    // ------------------------------------------------------------
    // Gerak & deteksi sentuhan objek
    // ------------------------------------------------------------
    void FixedUpdate()
    {
        float x = (right ? 1 : 0) - (left ? 1 : 0);
        float y = (up ? 1 : 0) - (down ? 1 : 0);

        // arah gerak mengikuti arah kamera
        Transform cam = Camera.main != null ? Camera.main.transform : transform;
        Vector3 fwd = cam.forward; fwd.y = 0; fwd.Normalize();
        Vector3 rgt = cam.right; rgt.y = 0; rgt.Normalize();
        Vector3 dir = Vector3.ClampMagnitude(fwd * y + rgt * x, 1f);

        if (rb != null)
            rb.linearVelocity = new Vector3(dir.x * speed, rb.linearVelocity.y, dir.z * speed);
        else
            transform.position += dir * speed * Time.fixedDeltaTime;
    }

    void OnCollisionEnter(Collision c) { Cek(c.gameObject); }
    void OnTriggerEnter(Collider o) { Cek(o.gameObject); }

    void Cek(GameObject g)
    {
        if (g.name == targetName)
        {
            success = true;
            Debug.Log("Target tersentuh. Praktikum berhasil.");
        }
        else if (g.name.StartsWith("Obstacle"))
        {
            Debug.Log("Obstacle tersentuh: " + g.name);
        }
    }

    // ------------------------------------------------------------
    // Tampilan
    // ------------------------------------------------------------
    void OnGUI()
    {
        if (panelStyle == null)
        {
            panelStyle = new GUIStyle();
            notifStyle = new GUIStyle();
        }

        // style di-set ulang setiap frame supaya pasti terpakai
        panelStyle.normal.background = texPanel;
        panelStyle.border = new RectOffset(20, 20, 20, 20);

        int fs = Mathf.RoundToInt(minSide * 0.04f);
        notifStyle.fontSize = fs;
        notifStyle.fontStyle = FontStyle.Bold;
        notifStyle.alignment = TextAnchor.MiddleCenter;
        notifStyle.wordWrap = true;
        notifStyle.normal.textColor = Color.white;

        // ---- Notifikasi (panel gelap di atas) ----
        float m = minSide * 0.04f;
        Rect box = new Rect(m, m, Screen.width - 2 * m, fs * 3.6f);
        GUI.color = new Color(0.15f, 0.19f, 0.27f, 0.95f);
        GUI.Box(box, GUIContent.none, panelStyle);
        GUI.color = Color.white;
        GUI.Label(box, success ? judul + "\n" + pesanSukses : judul, notifStyle);

        // ---- Tombol arah (atas, bawah, kiri, kanan) ----
        Tombol(rUp, 0f, up);
        Tombol(rRight, 90f, right);
        Tombol(rDown, 180f, down);
        Tombol(rLeft, 270f, left);

        // hiasan kecil di tengah susunan tombol
        float c = btnSize * 0.35f;
        Vector2 pusat = new Vector2(rUp.center.x, rLeft.center.y);
        GUI.color = new Color(1, 1, 1, 0.12f);
        GUI.DrawTexture(new Rect(pusat.x - c / 2, pusat.y - c / 2, c, c), texBulat);
        GUI.color = Color.white;
    }

    void Tombol(Rect r, float sudut, bool ditekan)
    {
        // isi lingkaran
        GUI.color = new Color(1, 1, 1, ditekan ? 0.55f : 0.25f);
        GUI.DrawTexture(r, texBulat);

        // garis tepi
        GUI.color = new Color(1, 1, 1, ditekan ? 1f : 0.7f);
        GUI.DrawTexture(r, texRing);

        // panah (diputar sesuai arah)
        float p = r.width * 0.5f;
        Rect pr = new Rect(r.center.x - p / 2, r.center.y - p / 2, p, p);
        Matrix4x4 lama = GUI.matrix;
        GUIUtility.RotateAroundPivot(sudut, r.center);
        GUI.color = new Color(1, 1, 1, ditekan ? 1f : 0.9f);
        GUI.DrawTexture(pr, texPanah);
        GUI.matrix = lama;
        GUI.color = Color.white;
    }
}