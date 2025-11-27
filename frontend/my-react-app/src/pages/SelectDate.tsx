import { useAuth } from "../context/AuthContext";

const SelectDate = () => {
  const { logout } = useAuth();

  return (
    <div style={{ padding: 20 }}>
      <h1>Вибір дат</h1>
      <p>Тут пізніше додамо дату "від", дату "до", кнопки для парсингу.</p>

      <button onClick={logout}>Вийти</button>
    </div>
  );
};

export default SelectDate;
