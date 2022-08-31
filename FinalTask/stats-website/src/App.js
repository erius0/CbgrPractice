import React from 'react';
import { BrowserRouter as Router, Switch, Route, Link } from 'react-router-dom';
import './App.css';

function App() {
  return (
    <Router>
      <header>
          <nav>
              <ul>
                  <li>
                      <Link to="/">Home</Link>
                  </li>
                  <li>
                      <Link to="/users">Users</Link>
                  </li>
                  <li>
                      <Link to="/messages">Messages</Link>
                  </li>
              </ul>
          </nav>
      </header>

      <main>
          <Switch>
              <Route path="/messages">
                  <Messages />
              </Route>
              <Route path="/users">
                  <Users />
              </Route>
              <Route path="/">
                  <Home />
              </Route>
          </Switch>
      </main>
    </Router>
  );
}

function Home() {
  return (
    <>
      <h2>Home</h2>
      <h3>Welcome to the push notification statistics website!</h3>
    </>
  );
}

class Users extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      error: null,
      isLoaded: false,
      userStats: {}
    }
  }

  componentDidMount() {
    fetch("https://localhost:8706/stats/user_apps")
      .then(res => res.json())
      .then(
        (result) => {
          this.setState({
            isLoaded: true,
            userStats: result
          });
        },
        (error) => {
          this.setState({
            isLoaded: true,
            error
          });
        }
      )
  }

  render() {
    const { error, isLoaded, userStats } = this.state;
    const pageCount = userStats ? Math.ceil(userStats.appUsers / 10) : 0;
    if (error) {
      return <div>Ошибка: {error.message}</div>;
    } else if (!isLoaded) {
      return <div>Загрузка...</div>;
    } else {
      return (
        <>
          <h2>Users</h2>
          <h3></h3>
          <table class='styled-table'>
            <thead>
              <tr>
                <th>App GUID</th>
                <th>User phone</th>
                <th>App version</th>
              </tr>
            </thead>
            <tbody>
            {userStats.appUsers.map(user => (
              <tr>
                <td>{user.appGuid}</td>
                <td>{user.phone}</td>
                <td>{user.version}</td>
              </tr>
            ))}
            </tbody>
          </table>
        </>
      );
    }
  }
}

const Messages = () => <h2>Messages</h2>;

export default App;
