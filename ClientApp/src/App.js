import React, {useState, useEffect} from 'react';
import {Route} from 'react-router';
import {Layout} from './components/Layout';
import {Home} from './components/Home';
import {FetchData} from './components/FetchData';
import {Counter} from './components/Counter';
import Setup from "./components/Settings/Setup/Setup";
import Search from "./components/Search/Search";
import Streamers from "./components/Streamers/Streamers";
import Streamer from "./components/Streamers/Streamer/Streamer";
import './custom.css'
import General from "./components/Settings/General/General";

export default function App() {
    
    
    return (
        <Layout>
            <Route exact path='/' component={Home}/>
            <Route path='/counter' component={Counter}/>
            <Route path='/fetch-data' component={FetchData}/>
            <Route path='/settings/setup' component={Setup}/>
            <Route path='/search' component={Search}/>
            <Route path='/streamers' component={Streamers}/>
            <Route path='/streamer/:id' component={Streamer}/>
            <Route path='/settings/general' component={General}/>
        </Layout>
    )
}
