import React, {useState, useEffect} from 'react';
import {HubConnectionBuilder, LogLevel} from "@microsoft/signalr";
import {AppBar, fade, InputBase, makeStyles, Toolbar, Typography} from "@material-ui/core";
import TopBarSearch from "./TopBarSearch";


const styles = makeStyles((theme) => ({
    root: {
        flexGrow: 1
    },
    title: {
        flexGrow: 1,
        display: "none",
        [theme.breakpoints.up('sm')]: {
            display: "block"
        }
    }
}))


export default function TopBar() {
    const [message, setMessage] = useState("");
    const classes = styles();

    useEffect(() => {
        const hubConnection = new HubConnectionBuilder().withUrl('/notificationhub')
            .configureLogging(LogLevel.Information)
            .build();

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

        hubConnection.on("ReceiveMessage", (message) => {
            setMessage(message);
        })
    })
    
    return (
        <div className={classes.root}>
            <AppBar position="static">
                <Toolbar>
                    <Typography className={classes.title}>
                        Voddy
                    </Typography>
                    <TopBarSearch />
                </Toolbar>
            </AppBar>
        </div>
    )
}