import React, {Component} from 'react';
import {Container} from 'reactstrap';
import NavMenu from './NavMenu';
import TopBar from "./TopBar";
import "./Layout.css"

export default function Layout(props) {
    //static displayName = Layout.name;

    return (
        <div>
            <TopBar />
            <NavMenu hubConnection={props.hubConnection} />
            <Container>
                {props.children}
            </Container>
        </div>
    );
}
