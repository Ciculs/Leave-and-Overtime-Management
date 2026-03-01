import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

<<<<<<< HEAD
import 'bootstrap/dist/css/bootstrap.min.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.mount('#app')
=======
createApp(App)
  .use(router)
  .mount('#app')
>>>>>>> main
