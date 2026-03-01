import OTRequest from "../views/OTRequest.vue"
import OTEdit from "../views/OTEdit.vue"
import OTList from "../views/OTList.vue"

import { createRouter, createWebHistory } from "vue-router"
import DashboardLayout from "../layouts/DashboardLayout.vue"

import Login from "../views/Login.vue"
import Dashboard from "../views/Dashboard.vue"


const routes = [{
        path: "/login",
        component: Login
    },
    {
        path: "/",
        component: DashboardLayout,
        redirect: "/dashboard",
        children: [{
                path: "dashboard",
                component: Dashboard
            },

            // ✅ My OT list
            {
                path: "my-ot",
                component: OTList
            },

            // ✅ Edit OT
            {
                path: "ot/edit/:id",
                component: OTEdit
            }
        ]
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

export default router