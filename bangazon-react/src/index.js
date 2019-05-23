import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import {BrowserRouter as Router} from "react-router-dom"
import Bangazon from "./Components/Bangazon"


ReactDOM.render(
    <Router>
        <Bangazon />
    </Router>
,
 document.getElementById('root')
 )