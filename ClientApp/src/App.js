import React, {useState, useEffect} from 'react';
import {Route} from 'react-router';
import {Layout} from './components/Layout';
import {Home} from './components/Home';
import {FetchData} from './components/FetchData';
import {Counter} from './components/Counter';
import Setup from "./components/Setup/Setup";
import Search from "./components/Search/Search";
import Streamers from "./components/Streamers/Streamers";
import Streamer from "./components/Streamers/Streamer/Streamer";
import './custom.css'

import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";


export default function App() {
    
    
    return (
        <Layout>
            <Route exact path='/' component={Home}/>
            <Route path='/counter' component={Counter}/>
            <Route path='/fetch-data' component={FetchData}/>
            <Route path='/setup' component={Setup}/>
            <Route path='/search' component={Search}/>
            <Route path='/streamers' component={Streamers}/>
            <Route path='/streamer/:id' component={Streamer}/>
        </Layout>
    )
}
