<script lang="ts">
import { Options } from "vue-class-component";
import { Vue } from 'vue-property-decorator'

@Options({
	components: {
	}
})
export default class AdminNavMenu extends Vue {
	async mounted() {
	}
}
</script>

<template>
	<nav class="admin-nav">
		<router-link to="/">Overview</router-link> |
		<router-link to="/proxyconfigs">Proxies</router-link> |
		<router-link to="/clients">Clients</router-link> |
		<router-link to="/serverconfig">Server config</router-link> |
		<router-link to="/jobs">Jobs</router-link> |
		<a href="/auth/logout">[Logout]</a>
	</nav>
</template>

<style scoped lang="scss">
.admin-nav {
    &__item {

    }
}
</style>
