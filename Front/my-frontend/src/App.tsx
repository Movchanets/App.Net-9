import { useEffect } from "react";
import { useUserStore } from "./store/userStore";

function App() {
  const { users, fetchUsers, loading, error } = useUserStore();

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  return (
    <div className="p-4">
      <h1 className="text-xl font-bold">Users Dashboard</h1>

      {loading && <p>Loading...</p>}
      {error && <p className="text-red-500">{error}</p>}

      <ul>
        {users.map((u) => (
          <li key={u.id}>
            {u.userName} ({u.email}) → {u.userRoles.join(", ")}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default App;
