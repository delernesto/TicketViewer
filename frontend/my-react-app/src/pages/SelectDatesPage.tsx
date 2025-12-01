import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

interface DateRangeResponse {
    minDate: string;
    maxDate: string;
}

export default function SelectDatesPage() {
    const navigate = useNavigate();

    const [loadingDates, setLoadingDates] = useState(true);
    const [minDate, setMinDate] = useState<string>("");
    const [maxDate, setMaxDate] = useState<string>("");

    const [selectedStart, setSelectedStart] = useState<string>("");
    const [selectedEnd, setSelectedEnd] = useState<string>("");

    const [isParsing, setIsParsing] = useState(false);

    // =============================
    // Функція форматування дати
    // =============================
    const formatDate = (iso: string) => {
        if (!iso) return "";
        const d = new Date(iso);
        return d.toLocaleDateString("uk-UA");
    };

    // =============================
    // Завантаження мін/макс дат
    // =============================
    const loadDates = async () => {
        try {
            const res = await fetch("https://localhost:7198/api/requests/dates");
            const data: DateRangeResponse = await res.json();

            setMinDate(data.minDate);
            setMaxDate(data.maxDate);

            // Встановлюємо відразу під діапазон
            setSelectedStart(data.minDate);
            setSelectedEnd(data.maxDate);
        } catch (e) {
            console.error("Error loading date range:", e);
        } finally {
            setLoadingDates(false);
        }
    };

    useEffect(() => {
        loadDates();
    }, []);

    // =============================
    // Запуск парсерів + оновлення дат
    // =============================
    const runParser = async () => {
        setIsParsing(true);
        try {
            const res = await fetch("https://localhost:7198/api/admin/update", {
                method: "POST"
            });

            const data = await res.json();
            console.log("Parser result:", data);

            if (!res.ok) {
                alert("Помилка парсингу: " + data.message);
            } else {
                alert("Дані успішно оновлено!");

                // 🔥 ПЕРЕЗАВАНТАЖУЄМО ДАТИ ПІСЛЯ ОНОВЛЕННЯ
                await loadDates();
            }
        } catch (e) {
            console.error("Parsing error:", e);
            alert("Помилка при запуску парсера");
        } finally {
            setIsParsing(false);
        }
    };

    // =============================
    // Перехід до Dashboard
    // =============================
    const goToDashboard = () => {
        navigate(`/dashboard?start=${selectedStart}&end=${selectedEnd}`);
    };

    if (loadingDates) {
        return <div className="p-5 text-xl">Завантаження діапазону дат…</div>;
    }

    return (
        <div style={styles.container}>
            <h1 style={styles.title}>Налаштування даних</h1>

            {/* Кнопка оновлення даних */}
            <button
                style={styles.buttonPrimary}
                onClick={runParser}
                disabled={isParsing}
            >
                {isParsing ? "Оновлення..." : "Оновити дані"}
            </button>

            {/* Діапазон дат */}
            <div style={styles.block}>
                <h2>Доступний діапазон:</h2>

                <p style={{ fontSize: "18px" }}>
                    Мінімальна дата: <b>{formatDate(minDate)}</b><br />
                    Максимальна дата: <b>{formatDate(maxDate)}</b>
                </p>

                <div style={{ marginTop: "10px" }}>
                  <label>Початкова дата:</label>
                    <input
                        type="date"
                        value={selectedStart}
                        min={minDate}
                        max={maxDate}
                        onChange={(e) => {
                            const v = e.target.value;
                            if (v < minDate) setSelectedStart(minDate);
                            else if (v > maxDate) setSelectedStart(maxDate);
                            else setSelectedStart(v);
                        }}
                        style={styles.input}
                    />

                    <label style={{ marginLeft: "20px" }}>Кінцева дата:</label>
                    <input
                        type="date"
                        value={selectedEnd}
                        min={minDate}
                        max={maxDate}
                        onChange={(e) => {
                            const v = e.target.value;
                            if (v < minDate) setSelectedEnd(minDate);
                            else if (v > maxDate) setSelectedEnd(maxDate);
                            else setSelectedEnd(v);
                        }}
                        style={styles.input}
                    />
                </div>
            </div>

            {/* Перейти */}
            <button
                style={styles.buttonSecondary}
                onClick={goToDashboard}
            >
                Перейти до BI Dashboard
            </button>
        </div>
    );
}

// ================================
// CSS (inline)
// ================================
const styles: Record<string, React.CSSProperties> = {
    container: {
        padding: "30px",
        maxWidth: "800px",
        margin: "0 auto",
        display: "flex",
        flexDirection: "column",
        gap: "20px"
    },
    title: {
        fontSize: "32px",
        fontWeight: "600",
        marginBottom: "20px"
    },
    block: {
        background: "#fafafa",
        padding: "20px",
        borderRadius: "10px",
        border: "1px solid #ddd",
        boxShadow: "0 2px 6px rgba(0,0,0,0.05)"
    },
    buttonPrimary: {
        padding: "12px 20px",
        background: "#007bff",
        color: "white",
        border: "none",
        cursor: "pointer",
        borderRadius: "8px",
        fontSize: "18px"
    },
    buttonSecondary: {
        padding: "12px 20px",
        background: "green",
        color: "white",
        border: "none",
        cursor: "pointer",
        borderRadius: "8px",
        fontSize: "18px",
        marginTop: "20px"
    },
    input: {
        padding: "8px",
        marginLeft: "5px",
        fontSize: "16px",
        borderRadius: "6px",
        border: "1px solid #ccc"
    }
};
