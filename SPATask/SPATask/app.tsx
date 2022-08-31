import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { BrowserRouter as Router, Switch, Route, Link } from 'react-router-dom';

export const App = () => (
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
                        <Link to="/stat">Stat</Link>
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
                <Route path="/stat">
                    <Stat />
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

const Home = () => <h2>Home</h2>;

const Users = () => <h2>Users</h2>;

const Stat = () => <h2>Stat</h2>;

const Messages = () => <h2>Messages</h2>;

ReactDOM.render(<App />, document.getElementById('root'));