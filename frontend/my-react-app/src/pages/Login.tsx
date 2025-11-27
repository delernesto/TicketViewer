import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import logo from "../assets/ukrposhta.png"; // ⬅️ Додаємо логотип

const Login = () => {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (username === "admin" && password === "admin") {
      login();
      navigate("/select");
    } else {
      setError("Невірний логін або пароль");
    }
  };

  return (
    <div style={styles.container}>
      <div style={styles.card}>
        
        <img src={logo} alt="Укрпошта" style={styles.logo} />   {/* ⬅️ Логотип */}

        <form onSubmit={handleSubmit} style={styles.form}>
          <h2>Вхід у систему</h2>

          <input
            type="text"
            placeholder="Логін"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            style={styles.input}
          />

          <input
            type="password"
            placeholder="Пароль"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            style={styles.input}
          />

          {error && <p style={{ color: "red" }}>{error}</p>}

          <button type="submit" style={styles.btn}>
            Увійти
          </button>
        </form>
      </div>
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  container: {
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    height: "100vh",
    background: "#f1f1f1",
  },
  card: {
    background: "white",
    padding: 40,
    borderRadius: 16,
    width: 420,
    textAlign: "center",
    boxShadow: "0 4px 20px rgba(0,0,0,0.15)",
  },
logo: {
    width: 280,
    marginBottom: 10, // 🔥 зменшили відступ
  },

  loginTitle: {
    marginTop: 10,    // 🔥 менше порожнього місця
    marginBottom: 20,
  },

  form: {
    display: "flex",
    flexDirection: "column",
    gap: 15,
  },
  input: {
    padding: "14px 12px",
    borderRadius: 6,
    border: "1px solid #ccc",
    fontSize: 16,        // ⬅️ більше текст
  },
  btn: {
    padding: "14px 12px",
    borderRadius: 6,
    border: "none",
    background: "#FFD200",
    color: "#222",
    fontWeight: "bold",
    fontSize: 17,         // ⬅️ трохи більша кнопка
    cursor: "pointer",
  },
};


export default Login;
