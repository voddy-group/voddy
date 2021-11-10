import React, {Component, useEffect} from 'react';
import NavMenu from './NavMenu/NavMenu';
import TopBar from "./TopBar";
import "./Layout.css"
import {Dialog, DialogContent, DialogContentText, DialogTitle, Container} from "@material-ui/core";

export default function Layout(props) {
    //static displayName = Layout.name;

    return (
        <div>
            <TopBar hubConnection={props.hubConnection}/>
            <NavMenu hubConnection={props.hubConnection}/>
            <Dialog
                open={props.hubDisconnected}
            >
                <DialogTitle>Server disconnected</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        The website has lost connection to the server. Please reload the website and if you continue
                        to see errors check the state of the server.
                    </DialogContentText>
                </DialogContent>
            </Dialog>
            <Container style={{paddingRight: 0, marginRight: 0, maxWidth: "90%"}}>
                {props.children}
            </Container>
        </div>
    );
}
