import React, {useEffect, useState} from 'react';
import {Route} from 'react-router-dom';
import Layout from './components/Layout';
import Setup from "./components/Settings/Setup/Setup";
import Search from "./components/Search/Search";
import Streamers from "./components/Streamers/Streamers";
import Streamer from "./components/Streamers/Streamer/Streamer";
import './custom.css'
import General from "./components/Settings/General/General";
import Quartz from "./components/Settings/Quartz/Quartz";
import {HubConnection, HubConnectionBuilder, LogLevel} from "@microsoft/signalr";
import Logs from "./components/Settings/Logs/logs";

export default function App() {
    const [hubDisconnected, setHubDisconnected] = useState(false);
    var hubConnection = new HubConnectionBuilder().withUrl('/notificationhub')
        .configureLogging(LogLevel.Information)
        .build();


    useEffect(() => {
        async function start() {
            try {
                await hubConnection.start();
                console.log("SignalR Connected.");
            } catch (err) {
                console.log(err);
                setTimeout(start, 5000);
            }
        }

        hubConnection.onclose(() => {
            setHubDisconnected(true);
            start();
        });
        hubConnection.onreconnected(() => {
            setHubDisconnected(false);
        });
        start();
    })

    return (
        <Layout hubConnection={hubConnection} hubDisconnected={hubDisconnected}>
            <Route exact path='/' render={() => <Streamers hubConnection={hubConnection} />} />
            <Route path='/search' component={Search}/>
            <Route exact path='/streamer/:id'
                   render={({match}) => <Streamer hubConnection={hubConnection} id={match.params.id}/>}/>
            <Route path='/settings/setup' component={Setup}/>
            <Route path='/settings/general' component={General}/>
            <Route path='/settings/quartz' component={Quartz}/>
            <Route path='/settings/logs' component={Logs}/>
        </Layout>
    )
}
