import React, {useEffect} from 'react';
import {Route} from 'react-router-dom';
import Layout from './components/Layout';
import Setup from "./components/Settings/Setup/Setup";
import Search from "./components/Search/Search";
import Streamers from "./components/Streamers/Streamers";
import Streamer from "./components/Streamers/Streamer/Streamer";
import './custom.css'
import General from "./components/Settings/General/General";
import {HubConnection, HubConnectionBuilder, LogLevel} from "@microsoft/signalr";

export default function App() {
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

        hubConnection.onclose(start);
        start();
    })
    
    return (
        <Layout hubConnection={hubConnection}>
            <Route exact path='/' component={Streamers}/>
            <Route path='/search' component={Search}/>
            <Route exact path='/streamer/:id' render={({match}) => <Streamer hubConnection={hubConnection} id={match.params.id}/>} />
            <Route path='/settings/setup' component={Setup}/>
            <Route path='/settings/general' component={General}/>
        </Layout>
    )
}
