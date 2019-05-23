import React, {Component} from "react";
import ApplicationViews from "./ApplicationViews"
import Navbar from "./Nav/Navbar"


class Bangazon extends Component {


    render() {
        return (
            <React.Fragment>
                <Navbar />
                <ApplicationViews />
            </React.Fragment>
        )
    }
}

export default Bangazon;