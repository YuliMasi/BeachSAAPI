namespace BeachSAAPI.Dto
{
    /// <summary>
    /// Respuesta del inicio de sesión.
    /// </summary>
    public class AuthorizationResponse
    {
        /// <summary>
        /// Token JWT con el correo y el rol del usuario.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Indica si las credenciales fueron válidas.
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Mensaje del resultado.
        /// </summary>
        public string Msj { get; set; }
    }
}
