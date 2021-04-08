import React, {useState, useEffect} from 'react';
import {Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink} from 'reactstrap';
import {Link} from 'react-router-dom';
import './NavMenu.css';
import {HubConnectionBuilder, LogLevel} from "@microsoft/signalr";

export default function NavMenu() {
    const [message, setMessage] = useState("");
    
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
        <header>
            <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
                <Container>
                    <NavbarBrand tag={Link} to="/">voddy</NavbarBrand>
                    <NavbarToggler className="mr-2"/>
                    <Collapse className="d-sm-inline-flex flex-sm-row-reverse" navbar>
                        <ul className="navbar-nav flex-grow">
                            <NavItem>
                                <NavLink tag={Link} className="text-dark" to="/">Home</NavLink>
                            </NavItem>
                            <NavItem>
                                <NavLink tag={Link} className="text-dark" to="/counter">Counter</NavLink>
                            </NavItem>
                            <NavItem>
                                <NavLink tag={Link} className="text-dark" to="/setup">Setup</NavLink>
                            </NavItem>
                            <NavItem>
                                <NavLink tag={Link} className="text-dark" to="/search">Search</NavLink>
                            </NavItem>
                            <NavItem>
                                <NavLink tag={Link} className="text-dark" to="/streamers">Streamers</NavLink>
                            </NavItem>
                        </ul>
                    </Collapse>
                    <p>{message}</p>
                </Container>
            </Navbar>
        </header>
    );
}
