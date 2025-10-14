using Microsoft.AspNetCore.Mvc;
using Domain.Entity;
using Repository.Repositories;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectRepository projectRepository, ILogger<ProjectsController> logger)
        {
            _projectRepository = projectRepository;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los proyectos
        /// </summary>
        /// <returns>Lista de proyectos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Project>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var projects = await _projectRepository.GetAllAsync();
            return Ok(projects);
        }

        /// <summary>
        /// Obtiene un proyecto por su ID
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Proyecto solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Project>> GetProject(Guid id)
        {
            var project = await _projectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }

        /// <summary>
        /// Obtiene un proyecto por su código
        /// </summary>
        /// <param name="code">Código del proyecto</param>
        /// <returns>Proyecto solicitado</returns>
        [HttpGet("by-code/{code}")]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Project>> GetProjectByCode(string code)
        {
            var project = await _projectRepository.GetByCodeAsync(code);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }
    }
}
