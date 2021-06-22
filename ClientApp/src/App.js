import React from 'react';
import {Route} from 'react-router-dom';
import {Layout} from './components/Layout';
import Setup from "./components/Settings/Setup/Setup";
import Search from "./components/Search/Search";
import Streamers from "./components/Streamers/Streamers";
import Streamer from "./components/Streamers/Streamer/Streamer";
import './custom.css'
import General from "./components/Settings/General/General";

export default function App() {
    return (
        <Layout>
            <Route exact path='/' component={Streamers}/>
            <Route path='/settings/setup' component={Setup}/>
            <Route path='/search' component={Search}/>
            <Route exact path='/streamer/:id' component={Streamer}/>
            <Route path='/settings/general' component={General}/>
        </Layout>
    )
}
